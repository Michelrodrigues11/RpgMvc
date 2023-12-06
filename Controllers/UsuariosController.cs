using Microsoft.AspNetCore.Mvc;
using RpgMvc.Models;
using System.Net.Http;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;

namespace RpgMvc.Controllers
{
    public class UsuariosController : Controller
    {
        public string uriBase = "http://lzsouza.somee.com/RpgApi/Usuarios/"; //

        [HttpGet]
        public ActionResult Index()
        {
            return View("CadastrarUsuario");
        }

        [HttpPost]
        public async Task<ActionResult> RegistrarAsync(UsuarioViewModel u)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                string uriComplementar = "Registrar";

                var content = new StringContent(JsonConvert.SerializeObject(u));
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
                    "application/json"
                );
                HttpResponseMessage response = await httpClient.PostAsync(
                    uriBase + uriComplementar,
                    content
                );

                string serialized = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    TempData["Mensagem"] = string.Format(
                        "Usuário {0} Registrado com sucesso! Faça o login para acessar.",
                        u.Username
                    );
                    return View("AutenticarUsuario");
                }
                else
                {
                    throw new System.Exception(serialized);
                }
            }
            catch (System.Exception ex)
            {
                TempData["MensagemErro"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public ActionResult IndexLogin()
        {
            return View("AutenticarUsuario");
        }

        [HttpPost]
        public async Task<ActionResult> AutenticarAsync(UsuarioViewModel u)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                string uriComplementar = "Autenticar";

                var content = new StringContent(JsonConvert.SerializeObject(u));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                HttpResponseMessage response = await httpClient.PostAsync(
                    uriBase + uriComplementar,
                    content
                );

                string serialized = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    UsuarioViewModel uLogado = JsonConvert.DeserializeObject<UsuarioViewModel>(
                        serialized
                    );
                    HttpContext.Session.SetString("SessionTokenUsuario", uLogado.Token);
                    HttpContext.Session.SetString("SessionUsername", uLogado.Username);

                    HttpContext.Session.SetString("SessionPerfilUsuario", uLogado.Perfil);
                    HttpContext.Session.SetString("SessionIdUsuario", uLogado.Id.ToString());

                    TempData["Mensagem"] = string.Format("Bem-vindo {0}!!!", uLogado.Username);
                    return RedirectToAction("Index", "Personagens");
                }
                else
                {
                    throw new System.Exception(serialized);
                }
            }
            catch (System.Exception ex)
            {
                TempData["MensagemErro"] = ex.Message;
                return IndexLogin();
            }
        }

        [HttpGet]
        public async Task<ActionResult> IndexInformacoesAsync()
        {
            try
            {
                HttpClient httpClient = new HttpClient();

                //Novo: Recuperação informação da sessão
                string login = HttpContext.Session.GetString("SessionUsername");
                string uriComplementar = $"GetByLogin/{login}";
                HttpResponseMessage response = await httpClient.GetAsync(uriBase + uriComplementar);
                string serialized = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    UsuarioViewModel u = await Task.Run(
                        () => JsonConvert.DeserializeObject<UsuarioViewModel>(serialized)
                    );
                    return View(u);
                }
                else
                {
                    throw new System.Exception(serialized);
                }
            }
            catch (System.Exception ex)
            {
                TempData["MensagemErro"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AlterarEmail(UsuarioViewModel u)
        {
            try
            {
                // Cria um objeto HttpClient
                HttpClient httpClient = new HttpClient();

                // Obtém o token de sessão do usuário
                string token = HttpContext.Session.GetString("SessionTokenUsuario");

                // Adiciona o token de autorização aos cabeçalhos HTTP
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                    "Bearer",
                    token
                );

                // Cria uma string com a URI da API
                string uriComplementar = "AtualizarEmail";

                // Cria um objeto StringContent com os dados do usuário
                var content = new StringContent(JsonConvert.SerializeObject(u));

                // Define o tipo de conteúdo do objeto StringContent
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                // Realiza uma chamada PUT à API
                HttpResponseMessage response = await httpClient.PutAsync(
                    uriBase + uriComplementar,
                    content
                );

                // Lê a resposta da API
                string serialized = await response.Content.ReadAsStringAsync();

                // Verifica o status da resposta
                if (response.StatusCode == System.Net.HttpStatusCode.OK)

                    // Adiciona uma mensagem de sucesso ao TempData
                    TempData["Mensagem"] = "E-mail alterado com sucesso.";

                else

                    // Lança uma exceção
                    throw new System.Exception(serialized);

            }
            catch (System.Exception ex)
            {
                // Adiciona uma mensagem de erro ao TempData
                TempData["MensagemErro"] = ex.Message;
            }

            // Retorna para a página de informações do usuário
            return RedirectToAction("IndexInformacoes");
        }

        [HttpGet]
        public async Task<ActionResult> ObterDadosAlteracaoSenha()
        {
            UsuarioViewModel viewModel = new UsuarioViewModel();
            try
            {
                HttpClient httpClient = new HttpClient();
                string login = HttpContext.Session.GetString("SessionUsername");
                string uriComplementar = $"GetByLogin/{login}";
                HttpResponseMessage response = await httpClient.GetAsync(uriBase + uriComplementar);
                string serialized = await response.Content.ReadAsStringAsync();
                TempData["TituloModalExterno"] = "Alteração de Senha";
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    viewModel = await Task.Run(
                        () => JsonConvert.DeserializeObject<UsuarioViewModel>(serialized)
                    );
                    return PartialView("_AlteracaoSenha", viewModel);
                }
                else
                    throw new System.Exception(serialized);
            }
            catch (System.Exception ex)
            {
                TempData["MensagemErro"] = ex.Message;
                return RedirectToAction("IndexInformacoes");
            }
        }
        [HttpPost]
        public async Task<ActionResult> AlterarSenha(UsuarioViewModel u)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                string token = HttpContext.Session.GetString("SessionTokenUsuario"); httpClient.DefaultRequestHeaders.Authorization = new
                AuthenticationHeaderValue("Bearer", token);
                string uriComplementar = "AlterarSenha";
                u.Username = HttpContext.Session.GetString("SessionUsername"); var content = new StringContent(JsonConvert.SerializeObject(u)); content.Headers.ContentType = new MediaTypeHeaderValue("application/json"); HttpResponseMessage response = await httpClient.PutAsync(uriBase + uriComplementar, content);
                string serialized = await response.Content.ReadAsStringAsync(); if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string mensagem = "Senha alterada com sucesso."; TempData["Mensagem"] = mensagem; //Mensagem guardada do TempData que aparcerá na página pai do modal
                    return Json(mensagem); //Mensagem que será exibida no alert da Função que chamou este método
                }
                else
                    throw new System.Exception(serialized);
            }
            catch (System.Exception ex)
            {
                return Json(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult> EnviarFoto(UsuarioViewModel u)
        {
            try
            {
                if (Request.Form.Files.Count == 0)
                    throw new System.Exception("Selecione o arquivo.");
                else
                {
                    var file = Request.Form.Files[0];
                    var fileName = Path.GetFileName(file.FileName);
                    string nomeArquivoSemExtensao = Path.GetFileNameWithoutExtension(fileName);
                    var extensao = Path.GetExtension(fileName);

                    if (extensao != ".jpg" && extensao != "jpeg" && extensao != ".png")
                        throw new System.Exception("O Arquivo selecionado não é uma foto.");

                    //var pastaUpload = @"\" + "";
                    //var path = Path.Combine(pastaUpload, fileName);
                    using (var ms = new MemoryStream())
                    {
                        file.CopyTo(ms);
                        u.Foto = ms.ToArray();
                        //string s = Convert.ToBase64String(fileBytes); // Escrever bytes numa string
                        //System.IO.File.WriteAllBytes(path, ms.ToArray()); //Escrever arquivo em uma pasta
                    }
                }

                HttpClient httpClient = new HttpClient();
                string token = HttpContext.Session.GetString("SessionTokenUsuario");
                httpClient.DefaultRequestHeaders.Authorization = new
                AuthenticationHeaderValue("Bearer", token);
                string uriComplementar = "AtualizarFoto";
                var content = new StringContent(JsonConvert.SerializeObject(u));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                HttpResponseMessage response = await httpClient.PutAsync(uriBase +
                uriComplementar, content);
                string serialized = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    TempData["Mensagem"] = "Foto enviada com sucesso";
                else
                    throw new System.Exception(serialized);
            }
            catch (System.Exception ex)
            {

                TempData["MensagemErro"] = ex.Message;
            }
            return RedirectToAction("IndexInformacoes");
        }
        [HttpGet]
        public async Task<ActionResult> BaixarFoto()
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                string login = HttpContext.Session.GetString("SessionUsername");
                string uriComplementar = $"GetByLogin/{login}";
                HttpResponseMessage response = await httpClient.GetAsync(uriBase +
                uriComplementar);
                string serialized = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    UsuarioViewModel viewModel = await
                    Task.Run(() =>
                    JsonConvert.DeserializeObject<UsuarioViewModel>(serialized));
                    //string contentType = "application/image";
                    string contentType = System.Net.Mime.MediaTypeNames.Application.Octet;
                    byte[] fileBytes = viewModel.Foto;
                    string fileName =
                    $"Foto{viewModel.Username}_{DateTime.Now:ddMMyyyyHHmmss}.png"; // + extensao;
                    return File(fileBytes, contentType, fileName);
                }
                else
                    throw new System.Exception(serialized);
            }
            catch (System.Exception ex)
            {
                TempData["MensagemErro"] = ex.Message;
                return RedirectToAction("IndexInformacoes");
            }
        }

        [HttpGet]
        public ActionResult Sair()
        {
            try
            {
                HttpContext.Session.Remove("SessionTokenUsuario");
                HttpContext.Session.Remove("SessionUsername");
                HttpContext.Session.Remove("SessionPerfilUsuario");
                HttpContext.Session.Remove("SessionIdUsuario");

                return RedirectToAction("Index", "Home");
            }
            catch (System.Exception ex)
            {

                TempData["MensagemErro"] = ex.Message;
                return RedirectToAction("IndexInformacoes");
            }

        }
    }
}
