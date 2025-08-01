# Tarefas ASP.NET

Projeto simples de **lista de tarefas** com funcionalidades de **gerenciamento e ordenação**, desenvolvido com **ASP.NET Core** e **SQL Server**.

## ✨ Funcionalidades

- ✅ Listar tarefas
- ➕ Adicionar nova tarefa
- ✏️ Editar tarefa
- ❌ Remover tarefa
- 🔃 Reordenar tarefas (movimentação para cima ou para baixo)

## 🛠 Tecnologias Utilizadas

- ASP.NET Core (.NET 9.0)
- SQL Server
- Razor Pages
- ADO.NET (SqlCommand/SqlConnection)
- HTML/CSS com layout em Razor (_Layout.cshtml)

## 💻 Estrutura do Projeto

```
TarefasASPNET/
├── Controllers/
│ └── HomeController.cs
├── DBConnection/
│ └── Config.cs
├── Models/
│ └── Tarefas.cs
├── Views/
│ ├── Home/
│ │ ├── Index.cshtml
│ │ └── Privacy.cshtml
│ └── Shared/
│ ├── _Layout.cshtml
│ └── Error.cshtml
├── wwwroot/
├── Program.cs
└── TarefasASPNET.csproj
```


## 🚀 Como executar localmente

1. Clone o repositório:

```bash
git clone https://github.com/DouglasProvk/TarefasASPNET.git
cd TarefasASPNET
```

2 - Configure a string de conexão em DBConnection/Config.cs para apontar para seu SQL Server local.

3 - Restaure os pacotes e compile o projeto:

```
dotnet restore
dotnet build
dotnet run
```

4 - Acesse em: http://localhost:5000

🧩 Observações
O projeto realiza comandos SQL diretamente com SqlCommand, sem usar ORM (como Entity Framework).

O banco de dados deve conter uma tabela de tarefas com os campos esperados (como Identificador_da_tarefa, Descricao, Ordem_de_apresentacao).

📄 Licença
Este projeto está sob a licença MIT. Sinta-se à vontade para usar e contribuir!

Desenvolvido por DouglasProvk
