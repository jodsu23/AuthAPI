using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Custom;
using WebAPI.Models;
using WebAPI.Models.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [AllowAnonymous]
    [ApiController]
    public class AccesoController : ControllerBase
    {
        //private readonly DbContext _dbPruebaContext;
        private readonly AppDbContext _dbPruebaContext;
        private readonly Utilidades _utilidades;

        //public AccesoController(DbContext dbPruebaContext, Utilidades utilidades)
        public AccesoController(AppDbContext dbPruebaContext, Utilidades utilidades)
        {
            //_dbPruebaContext = dbPruebaContext;
            _dbPruebaContext = dbPruebaContext;
            _utilidades = utilidades;
        }

        [HttpPost]
        [Route("Registrarse")]
        public async Task<IActionResult> Registrarse(UsuarioDTO objeto)
        {
            var modeloUsuario = new Usuario
            {
                Nombre = objeto.Nombre,
                Correo = objeto.Correo,
                //  Desde aca encriptamos la clave con la funcion creada
                Clave = _utilidades.encriptarSHA256(objeto.Clave)
            };

            //await _dbPruebaContext.Usuarios.AddAsync(modeloUsuario);
            await _dbPruebaContext.Usuarios.AddAsync(modeloUsuario);
            await _dbPruebaContext.SaveChangesAsync();

            if (modeloUsuario.IdUsuario != 0)
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = true });
            else
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = false });

        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login(LoginDTO objeto)
        {
            var usuarioEncontrado = await _dbPruebaContext.Usuarios
                .Where(u =>
                    u.Correo == objeto.Correo &&
                    u.Clave == _utilidades.encriptarSHA256(objeto.Clave)
                ).FirstOrDefaultAsync();

            if (usuarioEncontrado == null)
                return StatusCode(StatusCodes.Status200OK, new { isSucess = false, token = "" });
            else
                return StatusCode(StatusCodes.Status200OK, new { isSucess = true, token = _utilidades.generarJWT(usuarioEncontrado) });
        }

        //  Lo usamos para validar el token al final
        [HttpGet]
        [Route("ValidarToken")]
        public IActionResult ValidarToken([FromQuery] string token)
        {
            bool respuesta = _utilidades.validarToken(token);

            return StatusCode(StatusCodes.Status200OK, new { isSucess = respuesta });
        }
        

    }
}
