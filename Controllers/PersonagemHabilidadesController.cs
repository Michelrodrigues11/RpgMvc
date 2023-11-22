using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Collections.Generic;
using RpgMvc.Models;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace RpgMvc.Controllers
{
    public class PersonagemHabilidadesController : Controller
    {
        public string uriBase = "http://lzsouza.somee.com/RpgApi/PersonagemHabilidades/";
        //xyz será substituído pelo nome do seu site na API.

        [HttpGet("PersonagemHabilidades/{id}")]

        public async Task<ActionResult> IndexAsync(int id)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                string token = HttpContext.Session.GetString("SessionTokenUsuario");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                HttpResponseMessage response = await httpClient.GetAsync(uriBase + id.ToString());
                string serialized = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    List<PersonagemHabilidadeViewModel> lista = await Task.Run(() =>
                    JsonConvert.DeserializeObject<List<PersonagemHabilidadeViewModel>>(serialized));

                    return View(lista);
                }
                else
                    throw new System.Exception(serialized);

            }
            catch (System.Exception ex)
            {

                TempData["MensagemErro"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpGet("Delete/{habilidadesId}/{personagemId}")]
        public async Task<ActionResult> DeleteAsync(int habilidadeId, int personagemId)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                string uriComplementar = "DeletePersonagemHabilidade";
                string token = HttpContext.Session.GetString("SessionTokenUsuario");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                PersonagemHabilidadeViewModel ph = new PersonagemHabilidadeViewModel();
                ph.HabilidadeId = habilidadeId;
                ph.PersonagemId = personagemId;

                var content = new StringContent(JsonConvert.SerializeObject(ph));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                HttpResponseMessage response = await httpClient.PostAsync(uriBase + uriComplementar, content);
                string serialized = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    TempData["Mensagem"] = "Habilidades removida com sucesso";
                else
                    throw new System.Exception(serialized);

            }
            catch (System.Exception ex)
            {

                TempData["MensagemErro"] = ex.Message;
            }
            return RedirectToAction("Index", new {Id = personagemId});
        }

        [HttpGet]

        public async Task<ActionResult> CreateAsync(int id, string nome)
        {
            try
            {
                string uriComplementar = "GetHabilidades";
                HttpClient httpClient = new HttpClient();
                string token = HttpContext.Session.GetString("SessionTokenUsuario");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await httpClient.GetAsync(uriBase + uriComplementar);

                string serialized = await response.Content.ReadAsStringAsync();
                List<HabilidadeViewModel> habilidades = await Task.Run(() =>
                JsonConvert.DeserializeObject<List<HabilidadeViewModel>>(serialized));
                ViewBag.ListaHabilidades = habilidades;

                PersonagemHabilidadeViewModel ph = new PersonagemHabilidadeViewModel();
                ph.Personagem = new PersonagemViewModel();
                ph.Habilidade = new HabilidadeViewModel();
                ph.PersonagemId = id;
                ph.Personagem.Nome = nome;

                return View(ph);
            }
            catch (System.Exception ex)
            {
                
               TempData["MesagemErro"] = ex.Message;
               return RedirectToAction("Create", new {id, nome });
            }
        }

        [HttpPost]

        public async Task<ActionResult> CreateAsync(PersonagemHabilidadeViewModel ph)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
            string token = HttpContext.Session.GetString("SessionTokenUsuario");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var content = new StringContent(JsonConvert.SerializeObject(ph));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = await httpClient.PostAsync(uriBase, content);
            string serialized = await response.Content.ReadAsStringAsync();

            if(response.StatusCode == System.Net.HttpStatusCode.OK)
                TempData["Mensagem"] = "Habilidade cadastrada com sucesso";
            else
                throw new System.Exception(serialized);
            }
            catch (System.Exception ex)
            {
                
                TempData["MensagemErro"] = ex.Message;
            }
            return RedirectToAction("Index", new { id = ph.PersonagemId});
        }
    }
}