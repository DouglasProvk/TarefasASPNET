namespace TarefasASPNET.Models
{
    public class Tarefas
    {
        public int Id { get; set; } // Identificador único da tarefa
        public string Nome { get; set; } // Nome da tarefa
        //public string Descricao { get; set; } // Descrição da tarefa
        public double Custo { get; set; } // Custo associado à tarefa
        public DateTime DataLimite { get; set; } // Data de início da tarefa
        public int OrdemApresentacao { get; set; } // Ordem de apresentação da tarefa
    }

}
