using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using TarefasASPNET.Models;

namespace TarefasASPNET.Controllers
{
    public class HomeController : Controller
    {
            private readonly DatabaseService _databaseService;

            public HomeController(DatabaseService databaseService)
            {
                _databaseService = databaseService;
            }

            public async Task<IActionResult> Index()
            {
                var lista = new List<Tarefas>();

                using (var connection = _databaseService.GetConnection())
                {
                    await connection.OpenAsync();

                    var sql = @"SELECT * FROM Tarefas ORDER BY Ordem_de_apresentacao ASC";

                    using (var command = new SqlCommand(sql, connection))
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            lista.Add(new Tarefas
                            {
                                Id = Convert.ToInt32(reader["Identificador_da_tarefa"]),
                                Nome = reader["Nome_da_tarefa"].ToString(),
                                Custo = Convert.ToDouble(reader["Custo"]),
                                DataLimite = Convert.ToDateTime(reader["Data_limite"]),
                                OrdemApresentacao = Convert.ToInt32(reader["Ordem_de_apresentacao"])
                            });
                        }
                    }
                }

                return View("Index", lista);
            }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id) // Método para excluir uma tarefa
        {
            using (var connection = _databaseService.GetConnection())
            {
                await connection.OpenAsync();

                var sql = "DELETE FROM Tarefas WHERE Identificador_da_tarefa = @id";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    await command.ExecuteNonQueryAsync();
                }
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string nome, double custo, DateTime dataLimite)// Método para editar uma tarefa
        {
            using (var connection = _databaseService.GetConnection()) // Cria uma conexão com o banco de dados
            {
                await connection.OpenAsync();

                // Verifica se o novo nome já existe em outra tarefa
                var checkSql = @"SELECT COUNT(*) FROM Tarefas 
                         WHERE Nome_da_tarefa = @nome AND Identificador_da_tarefa != @id";

                using (var checkCmd = new SqlCommand(checkSql, connection))
                {
                    checkCmd.Parameters.AddWithValue("@nome", nome);
                    checkCmd.Parameters.AddWithValue("@id", id);

                    var count = (int)await checkCmd.ExecuteScalarAsync(); // Conta quantas tarefas têm o mesmo nome, exceto a tarefa atual
                    if (count > 0)
                    {
                        TempData["Erro"] = "Já existe uma tarefa com esse nome.";
                        return RedirectToAction("Index");
                    }
                }

                // Atualiza a tarefa
                var updateSql = @"UPDATE Tarefas SET 
                            Nome_da_tarefa = @nome, 
                            Custo = @custo, 
                            Data_limite = @dataLimite 
                          WHERE Identificador_da_tarefa = @id";

                using (var updateCmd = new SqlCommand(updateSql, connection))
                {
                    updateCmd.Parameters.AddWithValue("@nome", nome);
                    updateCmd.Parameters.AddWithValue("@custo", custo);
                    updateCmd.Parameters.AddWithValue("@dataLimite", dataLimite);
                    updateCmd.Parameters.AddWithValue("@id", id);

                    await updateCmd.ExecuteNonQueryAsync();
                }
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string nome, double custo, DateTime dataLimite) // Método para criar uma nova tarefa
        {
            using (var connection = _databaseService.GetConnection()) // Cria uma conexão com o banco de dados
            {
                await connection.OpenAsync();

                // Verifica se já existe tarefa com o mesmo nome
                var checkSql = "SELECT COUNT(*) FROM Tarefas WHERE Nome_da_tarefa = @nome";
                using (var checkCmd = new SqlCommand(checkSql, connection))
                {
                    checkCmd.Parameters.AddWithValue("@nome", nome);
                    var count = (int)await checkCmd.ExecuteScalarAsync();
                    if (count > 0)
                    {
                        TempData["Erro"] = "Já existe uma tarefa com esse nome.";
                        return RedirectToAction("Index");
                    }
                }

                // Descobre a última ordem de apresentação
                var ordemSql = "SELECT ISNULL(MAX(Ordem_de_apresentacao), 0) + 1 FROM Tarefas"; // Obtém a próxima ordem de apresentação
                int novaOrdem;
                using (var ordemCmd = new SqlCommand(ordemSql, connection))
                {
                    novaOrdem = (int)await ordemCmd.ExecuteScalarAsync();
                }

                // Insere a nova tarefa
                var insertSql = @"INSERT INTO Tarefas (Nome_da_tarefa, Custo, Data_limite, Ordem_de_apresentacao)
                  VALUES (@nome, @custo, @dataLimite, @ordem)";

                using (var insertCmd = new SqlCommand(insertSql, connection))
                {
                    insertCmd.Parameters.AddWithValue("@nome", nome);
                    insertCmd.Parameters.AddWithValue("@custo", custo);
                    insertCmd.Parameters.AddWithValue("@dataLimite", dataLimite);
                    insertCmd.Parameters.AddWithValue("@ordem", novaOrdem);

                    await insertCmd.ExecuteNonQueryAsync();
                }
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Mover(int id, string direcao) // Método para mover uma tarefa para cima ou para baixo na lista
        {
            using (var connection = _databaseService.GetConnection())
            {
                await connection.OpenAsync();

                // 1. Busca a tarefa atual
                var getAtualSql = @"SELECT Identificador_da_tarefa, Ordem_de_apresentacao 
                            FROM Tarefas WHERE Identificador_da_tarefa = @id";
                int ordemAtual = 0;

                using (var cmd = new SqlCommand(getAtualSql, connection))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using var reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        ordemAtual = Convert.ToInt32(reader["Ordem_de_apresentacao"]); // Obtém a ordem atual da tarefa
                    }
                    else return RedirectToAction("Index"); // tarefa não encontrada
                }

                // 2. Define a nova ordem
                int novaOrdem = direcao == "cima" ? ordemAtual - 1 : ordemAtual + 1;

                // 3. Busca a tarefa que está na nova posição
                var getOutraSql = @"SELECT Identificador_da_tarefa 
                            FROM Tarefas WHERE Ordem_de_apresentacao = @novaOrdem";
                int idOutra = 0;
                using (var cmd = new SqlCommand(getOutraSql, connection))
                {
                    cmd.Parameters.AddWithValue("@novaOrdem", novaOrdem);
                    var result = await cmd.ExecuteScalarAsync();
                    if (result == null) return RedirectToAction("Index"); // fora dos limites
                    idOutra = Convert.ToInt32(result);
                }

                // 4. Troca as ordens
                var updateSql = @"UPDATE Tarefas SET Ordem_de_apresentacao = CASE 
                                WHEN Identificador_da_tarefa = @id THEN @novaOrdem
                                WHEN Identificador_da_tarefa = @idOutra THEN @ordemAtual
                            END
                            WHERE Identificador_da_tarefa IN (@id, @idOutra)";
                using (var cmd = new SqlCommand(updateSql, connection))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@idOutra", idOutra);
                    cmd.Parameters.AddWithValue("@novaOrdem", novaOrdem);
                    cmd.Parameters.AddWithValue("@ordemAtual", ordemAtual);
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
