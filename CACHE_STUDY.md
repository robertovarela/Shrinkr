
# Estudo de Funcionamento do Cache - Projeto Shrinkr

Este documento detalha a implementação do cache em memória no projeto, explicando seu registro, funcionamento e estratégias de expiração.

## 1. Registro dos Serviços (Dependency Injection)

O cache é configurado na inicialização da aplicação, no arquivo `RDS.API/Common/BuilderExtension.cs`.

### `AddServiceContainer`

```csharp
public static void AddServiceContainer(this WebApplicationBuilder builder)
{
    // ... outros serviços
    builder.Services.AddMemoryCache();
}
```

*   A linha `builder.Services.AddMemoryCache()` registra o serviço de cache em memória padrão do ASP.NET Core.
*   Isso disponibiliza a interface `IMemoryCache` para ser injetada em qualquer serviço que precise de cache.

### `AddRepositories` (Padrão Decorator)

A mágica acontece no registro dos repositórios, onde o padrão de projeto **Decorator** é utilizado para "envelopar" o repositório principal com uma camada de cache.

```csharp
public static void AddRepositories(this WebApplicationBuilder builder)
{
    // 1. Registra a implementação concreta que acessa o banco de dados.
    builder.Services.AddScoped<ShortUrlRepository>();

    // 2. Registra o Decorator de Cache para a interface IShortUrlRepository.
    builder.Services.AddScoped<IShortUrlRepository>(provider =>
    {
        var inner = provider.GetRequiredService<ShortUrlRepository>();
        var cache = provider.GetRequiredService<IMemoryCache>();
        var logger = provider.GetRequiredService<ILogger<CachedShortUrlRepository>>();
        return new CachedShortUrlRepository(inner, cache, logger);
    });
}
```

**Como funciona:**

1.  `ShortUrlRepository` é o repositório "real", que sempre se comunica com o banco de dados.
2.  `CachedShortUrlRepository` é o nosso **Decorator**. Ele implementa a mesma interface `IShortUrlRepository`, mas internamente possui uma instância do `ShortUrlRepository` e do `IMemoryCache`.
3.  Quando um serviço (como o `UrlShorteningService`) solicita uma `IShortUrlRepository` via injeção de dependência, o contêiner de DI entrega a instância do `CachedShortUrlRepository`.
4.  O serviço consumidor não sabe que está falando com um cache; ele apenas usa os métodos da interface. O `CachedShortUrlRepository` decide se deve buscar os dados do cache ou do banco de dados.

## 2. Funcionamento do Cache (`CachedShortUrlRepository`)

O arquivo `RDS.Infraestructure/Repositories/CachedShortUrlRepository.cs` contém a lógica principal.

### Leitura (Sobrescrevendo o Banco)

O método `GetByIdAsync` é o principal ponto onde o cache se sobrepõe à leitura do banco.

```csharp
public async Task<ReadShortUrlDto?> GetByIdAsync(long id)
{
    // 1. Tenta buscar do cache primeiro
    if (cache.TryGetValue<ReadShortUrlDto>(CacheKey(id), out var cached))
    {
        logger.LogDebug("Cache hit for ShortUrl id={Id}", id);
        return cached; // Retorna direto do cache
    }

    // 2. Cache miss: busca do banco de dados
    logger.LogDebug("Cache miss for ShortUrl id={Id}, fetching from database", id);
    var fromDb = await inner.GetByIdAsync(id);

    // 3. Se encontrou no banco, armazena no cache para futuras requisições
    if (fromDb != null)
    {
        logger.LogDebug("ShortUrl id={Id} found in database, caching for 5 minutes", id);
        cache.Set(CacheKey(id), fromDb, _cacheOptions);
    }
    // ... (lógica para cache de nulos)

    return fromDb;
}
```

**Fluxo:**

1.  **Cache Hit**: Se o dado está no cache, ele é retornado imediatamente. A consulta ao banco de dados (`inner.GetByIdAsync`) **não acontece**.
2.  **Cache Miss**: Se o dado não está no cache, a consulta é feita no banco. O resultado é então armazenado no cache antes de ser retornado. A próxima chamada para o mesmo `id` resultará em um "Cache Hit".

### Gravação e Atualização

Nos métodos `AddAsync` e `UpdateAsync`, a estratégia é **Cache-Aside (Write-Through)**.

```csharp
public async Task<long> AddAsync(CreateShortUrlDto shortUrl)
{
    // 1. Grava sempre no banco de dados primeiro
    var addedId = await inner.AddAsync(shortUrl);
    if (addedId != 0)
    {
        // 2. Adiciona o novo registro ao cache
        logger.LogDebug("ShortUrl id={Id} added and cached", addedId);
        cache.Set(CacheKey(addedId), /* objeto a ser cacheado */, _cacheOptions);
    }
    return addedId;
}

public async Task UpdateAsync(UpdateShortUrlDto shortUrl)
{
    // 1. Atualiza sempre no banco de dados primeiro
    await inner.UpdateAsync(shortUrl);

    // 2. Atualiza o valor no cache para manter a consistência
    logger.LogDebug("ShortUrl id={Id} updated and cache refreshed", shortUrl.Id);
    cache.Set(CacheKey(shortUrl.Id), shortUrl, _cacheOptions);
}
```

**Fluxo:**

1.  A operação de escrita (INSERT/UPDATE) é sempre executada diretamente no banco de dados.
2.  Após a confirmação do banco, o novo dado (ou o dado atualizado) é colocado no cache. Isso garante que o cache não sirva dados desatualizados.

### Invalidação de Cache

O método `IncrementClickCountAsync` usa uma estratégia de invalidação.

```csharp
public async Task IncrementClickCountAsync(long id)
{
    // 1. Atualiza o contador no banco
    await inner.IncrementClickCountAsync(id);

    // 2. Remove o item do cache
    logger.LogDebug("ShortUrl id={Id} click count incremented, cache invalidated", id);
    cache.Remove(CacheKey(id));
}
```

**Por que invalidar e não atualizar?**

*   **Simplicidade e Segurança**: É mais simples e seguro remover o item. A próxima leitura (`GetByIdAsync`) buscará o dado fresco do banco (com a contagem de cliques atualizada) e o colocará de volta no cache.
*   **Evitar Race Conditions**: Tentar ler o valor, incrementá-lo e salvá-lo no cache poderia levar a inconsistências se múltiplas requisições chegassem ao mesmo tempo. A invalidação garante que a fonte da verdade (o banco) seja consultada.

## 3. Tempo de Expiração

O tempo de vida dos itens no cache é controlado por `MemoryCacheEntryOptions`.

```csharp
private readonly MemoryCacheEntryOptions _cacheOptions = new()
{
    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
    SlidingExpiration = TimeSpan.FromMinutes(2)
};
```

*   `AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)`:
    *   **Expiração Absoluta**.
    *   Define um tempo de vida máximo e fixo para um item no cache.
    *   Neste caso, 5 minutos após ser adicionado, o item **será removido**, não importa quantas vezes ele foi acessado nesse período.
    *   Útil para garantir que os dados não fiquem "velhos" demais.

*   `SlidingExpiration = TimeSpan.FromMinutes(2)`:
    *   **Expiração Deslizante**.
    *   Remove um item do cache se ele ficar inativo (não for acessado) por um determinado período.
    *   Neste caso, se um item no cache não for lido por 2 minutos, ele será removido.
    *   Se o item for acessado, o "cronômetro" de 2 minutos é reiniciado.
    *   Ideal para manter no cache dados que são acessados com frequência.

**Como funcionam juntos:**

O item será removido do cache na condição que ocorrer primeiro:
1.  Passarem-se 5 minutos desde sua criação (expiração absoluta).
2.  Ele ficar 2 minutos sem ser acessado (expiração deslizante).

Isso cria um balanço: dados populares permanecem no cache (graças à expiração deslizante), mas são forçados a uma atualização periódica (graças à expiração absoluta), garantindo que não fiquem indefinidamente obsoletos.
