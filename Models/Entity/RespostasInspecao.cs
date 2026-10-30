namespace PassoCerto.Web.Models.Entity
{
    public class RespostasInspecao
    {
        public long Id { get; set; }
        public long InspecaoId { get; set; }
        public long ItensId { get; set; }
        public CategoriasChecklist? Categoria_nome { get; set; }
        public CategoriasChecklist? CategoriaOrdem { get; set; } 
        public ItemChecklist? Item_titulo { get; set; }
        public ItemChecklist? Item_descricao { get; set; } 
        public ItemChecklist? ItemOrdem { get; set; }
        public string Resposta { get; set; } = string.Empty;
        public string Observacao { get; set; } = string.Empty;
        public DateTime CriadoEm { get; set; }
        public DateTime AtualizadoEm { get; set; }
    
    }
}