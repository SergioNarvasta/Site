﻿using Dapper;
using HDProjectWeb.Models;
using HDProjectWeb.Validaciones;
using HDProjectWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Formatters;
using HDProjectWeb.Models.Helps;

namespace HDProjectWeb.Controllers
{
    public class RQCompraController :Controller
    {
        private readonly IRepositorioRQCompra repositorioRQCompra;
        private readonly IServicioPeriodo servicioPeriodo;
        private readonly IServicioUsuario servicioUsuario;

        public RQCompraController(IRepositorioRQCompra repositorioRQCompra,IServicioPeriodo servicioPeriodo,IServicioUsuario servicioUsuario) 
        {
            this.repositorioRQCompra = repositorioRQCompra;
            this.servicioPeriodo     = servicioPeriodo;
            this.servicioUsuario = servicioUsuario;
        }

        [HttpGet]
        public IActionResult Crear()
        {
            var periodo = servicioPeriodo.ObtenerPeriodo();
            var date = DateTime.Now;
            ViewBag.periodo = periodo;
            ViewBag.fecha = date.ToString("yyyy-MM-ddThh:mm");    
            ViewBag.S10_usuario = servicioUsuario.ObtenerCodUsuario();
            ViewBag.Rco_numrco = servicioPeriodo.NroReq();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Crear(RQCompra rQCompra)
        {
           /* if (!ModelState.IsValid){ return View(rQCompra); }*/       
            rQCompra.Cia_codcia = servicioPeriodo.Compañia();
            rQCompra.Suc_codsuc = servicioPeriodo.Sucursal();
            rQCompra.Ano_codano = servicioPeriodo.Ano();
            rQCompra.Mes_codmes = servicioPeriodo.Mes();
            rQCompra.S10_usuario = servicioUsuario.CodUsuario();
            rQCompra.Rco_usucre = servicioUsuario.ObtenerCodUsuario();
            rQCompra.Rco_codusu = servicioUsuario.ObtenerCodUsuario();
            await repositorioRQCompra.Crear(rQCompra);
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string periodo,string busqueda,string estado,string orden) 
        {
            if(periodo is not null)
            {
               await servicioPeriodo.ActualizaPeriodo(periodo);
            }
            if (orden is not null)
            {
                await servicioPeriodo.ActualizaOrden(orden);
            }
            orden =await servicioPeriodo.ObtenerOrden();
            periodo = await servicioPeriodo.ObtenerPeriodo();
            ViewBag.periodo = periodo.Remove(4, 2) + "-" + periodo.Remove(0, 4);
            PaginacionViewModel paginacionViewModel = new PaginacionViewModel();
            string CodUser = servicioUsuario.ObtenerCodUsuario();        
            string estado1, estado2;
            if (busqueda is not null)
            {
                if(estado == "2")
                {
                     estado1 = "1"; estado2 = "0";
                }else{
                    estado1 = estado; estado2 = estado;
                }
                var bus_rQCompra = await repositorioRQCompra.BusquedaMultiple(periodo, paginacionViewModel, CodUser, busqueda, estado1,estado2);
                var bus_totalRegistros = await repositorioRQCompra.ContarRegistrosBusqueda(periodo, CodUser, busqueda, estado1, estado2);
                var respuesta = new PaginacionRespuesta<RQCompraCab>
                {
                    Elementos = bus_rQCompra,
                    Pagina = paginacionViewModel.Pagina,
                    RecordsporPagina = paginacionViewModel.RecordsPorPagina,
                    CantidadRegistros = bus_totalRegistros,
                    BaseURL = Url.Action()
                };
                return View(respuesta);
            }
            else
            {
                var rQCompra = await repositorioRQCompra.Obtener(periodo, paginacionViewModel, CodUser, orden);
                var totalRegistros = await repositorioRQCompra.ContarRegistros(periodo, CodUser);
                var respuesta = new PaginacionRespuesta<RQCompraCab>
                {
                    Elementos = rQCompra,
                    Pagina = paginacionViewModel.Pagina,
                    RecordsporPagina = paginacionViewModel.RecordsPorPagina,
                    CantidadRegistros = totalRegistros,
                    BaseURL = Url.Action()
                };
                return View(respuesta);
            }                        
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Index(PaginacionViewModel paginacionViewModel)
        {
            string orden = await servicioPeriodo.ObtenerOrden();
            string CodUser = servicioUsuario.ObtenerCodUsuario();
            string periodo = await servicioPeriodo.ObtenerPeriodo();
            ViewBag.periodo =  periodo.Remove(4,2)+"-"+periodo.Remove(0,4);         
            var rQCompra   = await repositorioRQCompra.Obtener(periodo,paginacionViewModel,CodUser,orden);
            var totalRegistros = await repositorioRQCompra.ContarRegistros(periodo, CodUser);
            var respuesta = new PaginacionRespuesta<RQCompraCab>
            {
                Elementos = rQCompra,
                Pagina = paginacionViewModel.Pagina,
                RecordsporPagina = paginacionViewModel.RecordsPorPagina,
                CantidadRegistros = totalRegistros,
                BaseURL = Url.Action()
            };
            return View(respuesta);
        }
        [HttpGet]
        public async Task<IActionResult> AyudaDisciplina()
        {
            var Disciplina = await repositorioRQCompra.ObtenerDisciplina();
            var Data = new ListadoDisciplina<Disciplina>
            {
                Elementos = Disciplina
            };
            return PartialView(Data);
        }

        [HttpGet]
        public async Task<IActionResult> Editar(string Rco_Numero)
        {
            var rQCompra = await repositorioRQCompra.ObtenerporCodigo(Rco_Numero);
           /* if(rQCompra is null)
            {
                return RedirectToAction("NoEncontrado","Home");   
            }*/
            return View(rQCompra);
        }

        [HttpPost]
        public  async Task<IActionResult> Editar(RQCompra rQCompraEd)
        {
           //var usuarioid=servicioPeriodo.ObtenerPeriodo(); //ObtenerUsuarioId
           await repositorioRQCompra.Actualizar(rQCompraEd);
           return RedirectToAction("Index");
        }
    }
}
