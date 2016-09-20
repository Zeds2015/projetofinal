﻿using Microsoft.AspNet.Identity.Owin;
using ProjetoFinalWeb.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ProjetoFinalWeb.Controllers
{
    public class UsuarioController : Controller
    {
        private ApplicationDbContext context = new ApplicationDbContext();

        public async Task<ActionResult> Index(PlaylistModel play)
        {
            var userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var user = await userManager.FindByNameAsync(User.Identity.Name);

            if (context.Playlists.Count() > 0)
            {
                var lista = context.Playlists.Where(lambda => lambda.UsuarioId == user.Id || lambda.Titulo == "Ver depois");
                return View(lista);
            }

            return View(Enumerable.Empty<PlaylistModel>());
        }


        // GET: Usuario
        public ActionResult CriarPlaylist()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> CriarPlaylist(PlaylistModel play)
        {
            var userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var user = await userManager.FindByNameAsync(User.Identity.Name);

            var playlist = new PlaylistModel
            {
                Titulo = play.Titulo,
                UsuarioId = user.Id,
                Padrao = false,
                Publica = play.Publica,
                DataCriacao = DateTime.Now,
            };

            var existPlaylist = !context.Playlists.Any(x => x.Titulo == play.Titulo);

            if (existPlaylist)
            {
                context.Playlists.Add(playlist);
                await context.SaveChangesAsync();
            }
            else
            {
                ViewBag.Erro = string.Format("Playlist {0} já existente!", play.Titulo);
                return PartialView("ErroPlaylistExistente");
                //return Json(new
                //{
                //    Success = false,
                //    Message = string.Format("A Playlist {0} já existe.", play.Titulo)
                //    //ajustar com layout padrao do erro do sistema
                //});

            }
            return RedirectToAction("Index", "Home");
        }



        public ActionResult Detalhes(Guid? id)
        {
            if (id == null)
            {
                context.Dispose();
                return HttpNotFound();
            }

            var play = context.Playlists.Find(id);

            if (play == null)
            {
                context.Dispose();
                return HttpNotFound();
            }

            return View(play);
        }



        public ActionResult Editar(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var playlistAtual = context.Playlists.Find(id);

            if (playlistAtual == null)
            {
                return HttpNotFound();
            }
            return View(playlistAtual);
        }


        [HttpPost, ActionName("Editar")]
        public async Task<ActionResult> EditarPlaylist(PlaylistModel play)
        {
            try
            {
                context.Entry(play).State = EntityState.Modified;
                await context.SaveChangesAsync();
            }
            catch (DataException)
            {
                ViewBag.Erro = "Não consiguimos alterar a playlist";
            }
            return RedirectToAction("Index");
        }
    }
}