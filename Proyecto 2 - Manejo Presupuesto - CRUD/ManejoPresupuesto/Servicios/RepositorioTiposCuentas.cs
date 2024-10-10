using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;
using System.Collections;

namespace ManejoPresupuesto.Servicios
{
    public interface IRepositorioTiposCuentas
    {
        Task Actualizar(TipoCuentas tipoCuenta);
        Task Borrar(int id);
        Task Crear(TipoCuentas tipoCuentas);
        Task<bool> Existe(string nombre, int usuarioId);
        Task<IEnumerable<TipoCuentas>> Obtener(int usuarioId);
        Task<TipoCuentas> ObtenerPorId(int id, int usuarioId);
        Task Ordenar(IEnumerable<TipoCuentas> tipoCuentasOrdenadas);
    }


    public class RepositorioTiposCuentas : IRepositorioTiposCuentas
    {
        private readonly string ConnectionString;

        #region Conexion Base Datos
        public RepositorioTiposCuentas(IConfiguration configuration)
        {
            ConnectionString = configuration.GetConnectionString("ConexionSql");
        }
        #endregion

        #region Crear Tipos Cuentas
        public async Task Crear(TipoCuentas tipoCuentas)
        {
            using var connection = new SqlConnection(ConnectionString);
            var id = await connection.QuerySingleAsync<int>
                                                ("TiposCuentas_Insertar",   //llamando a un sp
                                                new { usuarioId = tipoCuentas.UsuarioId,
                                                nombre = tipoCuentas.Nombre}, 
                                                commandType: System.Data.CommandType.StoredProcedure);

            tipoCuentas.Id = id;
        }
        #endregion

        #region Verificar Si existe Tipo Cuenta
        public async Task<bool> Existe(string nombre, int usuarioId)
        {
            using var connection = new SqlConnection(ConnectionString);
            var existe = await connection.QueryFirstOrDefaultAsync<int>(
                                          @"SELECT 1
                                          FROM TiposCuentas
                                          WHERE Nombre = @Nombre AND UsuarioId = @UsuarioId;",
                                          new { nombre, usuarioId });
            return existe == 1;
        }
        #endregion

        #region Obtener Todos los Tipos Cuentas
        public async Task<IEnumerable<TipoCuentas>> Obtener(int usuarioId)
        {
            using var connection = new SqlConnection(ConnectionString);
            return await connection.QueryAsync<TipoCuentas>(@"SELECT Id, Nombre, Orden
                                                            FROM TiposCuentas
                                                            WHERE UsuarioId = @usuarioId
                                                            ORDER BY Orden", new { usuarioId });
        }
        #endregion

        #region Actualizar
        public async Task Actualizar(TipoCuentas tipoCuenta)
        {
            using var connection = new SqlConnection(ConnectionString);
            await connection.ExecuteAsync(@"UPDATE TiposCuentas
                                            SET Nombre = @Nombre
                                            WHERE Id = @Id", tipoCuenta);
        }
        #endregion

        #region ObtenerPorId
        public async Task<TipoCuentas> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(ConnectionString);
            return await connection.QueryFirstOrDefaultAsync<TipoCuentas>(@"
                                                                SELECT Id, Nombre, Orden
                                                                FROM TiposCuentas
                                                                WHERE Id = @Id AND UsuarioId = @UsuarioId",
                                                                new { id, usuarioId });
        }
        #endregion

        #region Eliminar TipoCuenta
        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(ConnectionString);
            await connection.ExecuteAsync("DELETE TiposCuentas WHERE Id = @Id", new { id });
        }
        #endregion

        #region Ordenar los tipos de elementos
        public async Task Ordenar(IEnumerable<TipoCuentas> tipoCuentasOrdenadas)
        {
            var query = "UPDATE TiposCuentas SET Orden = @Orden WHERE Id = @Id;";
            using var connection = new SqlConnection(ConnectionString);
            await connection.ExecuteAsync(query, tipoCuentasOrdenadas);
        }
        #endregion
    }
}
