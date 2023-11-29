using System.Collections.Generic;
using System;

namespace RpgMvc.Models;
public class DisputaViewModel
{
public int Id { get; set; }
public DateTime? DataDisputa { get; set; }
public int? AtacanteId { get; set; } = 0;
public int? OponenteId { get; set; } = 0;
public string Narracao { get; set; } = string.Empty;
public int? HabilidadeId { get; set;} = 0;
public List<int> ListaIdPersonagens { get; set; } = new List<int>();
public List<string> Resultados { get; set; } = new List<string>();
}

