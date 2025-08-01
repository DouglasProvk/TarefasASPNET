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


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
