# Encurtador de URL com .NET 9 e Blazor



## 🎯 Sobre o Projeto

Este é um projeto de estudo desenvolvido para demonstrar a construção de uma aplicação web moderna e performática utilizando uma arquitetura de microsserviços com .NET 9. A aplicação consiste em um serviço de encurtamento de URLs, com um backend robusto (API) e um frontend interativo (Blazor WebAssembly).

O objetivo foi aplicar conceitos de arquitetura limpa, separação de responsabilidades e boas práticas de desenvolvimento, como o uso de injeção de dependência, padrão repositório e comunicação segura entre serviços.

## ✨ Funcionalidades

*   **Encurtamento de URL:** Insira uma URL longa e receba uma URL curta e única.
*   **Redirecionamento:** Acesse a URL curta no navegador para ser redirecionado instantaneamente para a URL original.
*   **Interface Limpa:** Frontend desenvolvido com Blazor WebAssembly e a biblioteca de componentes MudBlazor, oferecendo uma experiência de usuário moderna e responsiva.
*   **Validação em Tempo Real:** A interface valida o formato da URL para evitar erros.
*   **Copiar para Área de Transferência:** Botão para copiar facilmente a URL encurtada.

## 🛠️ Tecnologias Utilizadas

### Backend (RDS.API)
*   **.NET 9:** A mais recente versão da plataforma de desenvolvimento da Microsoft.
*   **ASP.NET Core Web API:** Para a construção de endpoints RESTful.
*   **Entity Framework Core 9:** ORM para interação com o banco de dados.
*   **SQL Server:** Banco de dados relacional para persistência das URLs.
*   **Hashids.NET:** Biblioteca para gerar hashes curtos, únicos e não sequenciais a partir dos IDs do banco de dados.
*   **Swagger/OpenAPI:** Para documentação e teste interativo da API.

### Frontend (RDS.WEB)
*   **Blazor WebAssembly:** Para criar uma Single-Page Application (SPA) interativa que roda diretamente no navegador.
*   **MudBlazor:** Biblioteca de componentes de Material Design para Blazor.
*   **IHttpClientFactory:** Para gerenciar instâncias de `HttpClient` de forma eficiente e resiliente na comunicação com a API.

## 🏛️ Destaques da Arquitetura

*   **Separação de Responsabilidades:** O projeto é dividido em duas aplicações independentes:
    1.  `RDS.API`: Responsável por toda a lógica de negócio, validação e acesso a dados.
    2.  `RDS.WEB`: Responsável exclusivamente pela apresentação e experiência do usuário.
*   **Comunicação via API:** O frontend (Blazor) consome o backend (API) através de chamadas HTTP, uma abordagem padrão em arquiteturas de microsserviços.
*   **Configuração de CORS:** A API foi configurada com uma política de CORS (Cross-Origin Resource Sharing) para permitir requisições do domínio onde o aplicativo Blazor está hospedado, resolvendo um desafio comum em aplicações web distribuídas.
*   **Injeção de Dependência (DI):** Utilizada extensivamente em ambos os projetos para promover baixo acoplamento e alta testabilidade.
*   **Padrão Repositório:** A camada de acesso a dados é abstraída através do padrão repositório, facilitando a manutenção e a troca da tecnologia de persistência se necessário.

## 🚀 Como Executar o Projeto

### Pré-requisitos
*   .NET 9 SDK
*   Um editor de código como Visual Studio, Rider ou VS Code.
*   SQL Server (Express, Developer ou outra edição).

### 1. Configuração do Banco de Dados
1.  Abra o arquivo `appsettings.json` no projeto `RDS.API`.
2.  Altere a `ConnectionString` "DefaultConnection" para apontar para a sua instância do SQL Server.
3.  Abra um terminal na pasta do projeto `RDS.API` e execute as migrações do Entity Framework para criar o banco de dados e as tabelas:
     
