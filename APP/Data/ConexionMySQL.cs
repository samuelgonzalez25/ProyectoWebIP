using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using APP.Models;

namespace APP.Data
{
    public class ConexionMySql
    {
        private readonly string connectionString;

        public ConexionMySql()
        {
            connectionString = "Server=localhost;Database=MundoConsolasBD;User ID=root;Password=123qwe;Port=3306;SslMode=Preferred;";
        }

        public MySqlConnection GetConnection() => new MySqlConnection(connectionString);

        // ======================= LOGIN =======================
        public Usuario ObtenerUsuario(string usuario, string contraseña)
        {
            Usuario user = null;

            try
            {
                using var conn = GetConnection();
                conn.Open();
                // ✅ Ya no necesitamos el idUSER
                string query = "SELECT username, rol FROM user WHERE username=@usuario AND password=@password LIMIT 1";
                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@usuario", usuario);
                cmd.Parameters.AddWithValue("@password", contraseña);

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    user = new Usuario
                    {
                        // ❌ Ya no usamos Id
                        Username = reader.GetString("username"),
                        Rol = reader.GetString("rol")
                    };

                    Console.WriteLine($"✅ Usuario encontrado: {user.Username}, Rol: {user.Rol}");
                }
                else
                {
                    Console.WriteLine("❌ No se encontró usuario");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al obtener usuario: " + ex.Message);
            }

            return user;
        }


        // ======================= OBTENER USUARIO POR USERNAME =======================
        public Usuario ObtenerUsuarioPorUsername(string username)
        {
            Usuario user = null;

            try
            {
                using var conn = GetConnection();
                conn.Open();
                string query = "SELECT idUSER, username, rol FROM user WHERE username=@usuario LIMIT 1";
                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@usuario", username);

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    user = new Usuario
                    {
                        Id = reader.GetInt32("idUSER"),
                        Username = reader.GetString("username"),
                        Rol = reader.GetString("rol")
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al buscar usuario por username: " + ex.Message);
            }

            return user;
        }

        // ======================= REGISTRO =======================
        public bool RegistrarUsuario(
    string username,
    string password,
    string nombre,
    string apellido,
    string sexo,
    string nivelAcademico,
    string institucion,
    string rol,
    out string mensajeError)
        {
            mensajeError = "";
            try
            {
                using (var conn = this.GetConnection())
                {
                    conn.Open();

                    // 1️⃣ Verificar si el username ya existe
                    string checkUser = "SELECT COUNT(*) FROM user WHERE username=@username";
                    using (var cmdCheck = new MySqlCommand(checkUser, conn))
                    {
                        cmdCheck.Parameters.AddWithValue("@username", username);
                        int count = Convert.ToInt32(cmdCheck.ExecuteScalar());
                        if (count > 0)
                        {
                            mensajeError = $"Error: el username '{username}' ya existe.";
                            return false;
                        }
                    }

                    // 2️⃣ Obtener o insertar Nivel_Academico
                    int nivelAcademicoId;
                    string queryNivel = "SELECT idNivel FROM Nivel_Academico WHERE Nivel=@nivel LIMIT 1";
                    using (var cmdNivel = new MySqlCommand(queryNivel, conn))
                    {
                        cmdNivel.Parameters.AddWithValue("@nivel", nivelAcademico);
                        var result = cmdNivel.ExecuteScalar();
                        if (result != null)
                        {
                            nivelAcademicoId = Convert.ToInt32(result);
                        }
                        else
                        {
                            // Insertar nivel si no existe
                            string insertNivel = "INSERT INTO Nivel_Academico (Nivel, Institucion) VALUES (@nivel, @institucion)";
                            using (var cmdInsertNivel = new MySqlCommand(insertNivel, conn))
                            {
                                cmdInsertNivel.Parameters.AddWithValue("@nivel", nivelAcademico);
                                cmdInsertNivel.Parameters.AddWithValue("@institucion", institucion);
                                cmdInsertNivel.ExecuteNonQuery();
                                nivelAcademicoId = Convert.ToInt32(cmdInsertNivel.LastInsertedId);
                            }
                        }
                    }

                    // 3️⃣ Insertar en Generales
                    int idGeneral;
                    string insertGeneral = @"INSERT INTO Generales (nombre, apellido, sexo, Nivel_Academico_idNivel)
                                     VALUES (@nombre, @apellido, @sexo, @nivelAcademicoId)";
                    using (var cmd = new MySqlCommand(insertGeneral, conn))
                    {
                        cmd.Parameters.AddWithValue("@nombre", nombre);
                        cmd.Parameters.AddWithValue("@apellido", apellido);
                        cmd.Parameters.AddWithValue("@sexo", sexo);
                        cmd.Parameters.AddWithValue("@nivelAcademicoId", nivelAcademicoId);
                        cmd.ExecuteNonQuery();
                    }

                    // 4️⃣ Obtener último ID de Generales
                    using (var cmd = new MySqlCommand("SELECT LAST_INSERT_ID();", conn))
                    {
                        idGeneral = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    // 5️⃣ Insertar en user
                    string insertUsuario = @"INSERT INTO user (username, password, rol, Generales_idGeneral)
                                     VALUES (@username, @password, @rol, @idGeneral)";
                    using (var cmd = new MySqlCommand(insertUsuario, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        cmd.Parameters.AddWithValue("@password", password);
                        cmd.Parameters.AddWithValue("@rol", rol);
                        cmd.Parameters.AddWithValue("@idGeneral", idGeneral);
                        cmd.ExecuteNonQuery();
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                mensajeError = "Error: " + ex.Message;
                return false;
            }
        }

        // ======================= OBTENER TODAS LAS CONSOLAS CON PROVEEDORES =======================
        public List<Consola> ObtenerConsolas()
        {
            var consolas = new List<Consola>();

            try
            {
                using (var conn = this.GetConnection())
                {
                    conn.Open();

                    string query = @"
                SELECT c.idConsola, c.Marca, c.Modelo, c.Almacenamiento, c.Generacion, c.Incluye_Juegos, c.Imagen, c.Precio,
                       p.idProveedor, p.Nombre AS ProveedorNombre, p.Direccion, p.Telefono, p.Email
                FROM Consolas c
                LEFT JOIN Proveedores p ON c.Proveedores_idProveedor = p.idProveedor
            ";

                    using (var cmd = new MySqlCommand(query, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var consola = new Consola
                            {
                                IdConsola = reader.GetInt32("idConsola"),
                                Marca = reader.GetString("Marca"),
                                Modelo = reader.GetString("Modelo"),
                                Almacenamiento = reader.GetString("Almacenamiento"),
                                Generacion = reader.GetString("Generacion"),
                                IncluyeJuegos = reader.GetBoolean("Incluye_Juegos"),
                                Imagen = reader.GetString("Imagen"),
                                Precio = reader.GetDecimal("Precio"),
                                ProveedoresIdProveedor = reader.GetInt32("idProveedor"),
                                Proveedor = new Proveedor
                                {
                                    IdProveedor = reader.GetInt32("idProveedor"),
                                    Nombre = reader.GetString("ProveedorNombre"),
                                    Direccion = reader.IsDBNull(reader.GetOrdinal("Direccion")) ? "" : reader.GetString("Direccion"),
                                    Telefono = reader.IsDBNull(reader.GetOrdinal("Telefono")) ? "" : reader.GetString("Telefono"),
                                    Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? "" : reader.GetString("Email")
                                }
                            };

                            consolas.Add(consola);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al obtener consolas: " + ex.Message);
            }

            return consolas;
        }


        // ======================= EDITAR CONSOLA =======================
        public bool EditarConsola(Consola consola, Proveedor nuevoProveedor = null)
        {
            const string insertProveedorSql = @"
        INSERT INTO Proveedores (Nombre, Direccion, Telefono, Email)
        VALUES (@Nombre, @Direccion, @Telefono, @Email);
        SELECT LAST_INSERT_ID();
    ";

            const string updateConsolaSql = @"
        UPDATE Consolas SET
            Marca                   = @Marca,
            Modelo                  = @Modelo,
            Almacenamiento          = @Almacenamiento,
            Generacion              = @Generacion,
            Incluye_Juegos          = @IncluyeJuegos,
            Imagen                  = @Imagen,
            Precio                  = @Precio,
            Proveedores_idProveedor = @ProveedorId
        WHERE idConsola = @IdConsola;
    ";

            try
            {
                using var conn = GetConnection();
                conn.Open();
                using var tx = conn.BeginTransaction();

                // 1) Insertar proveedor si aplica
                if (nuevoProveedor != null)
                {
                    using var cmdProv = new MySqlCommand(insertProveedorSql, conn, tx);
                    cmdProv.Parameters.Add("@Nombre", MySqlDbType.VarChar, 100).Value = nuevoProveedor.Nombre;
                    cmdProv.Parameters.Add("@Direccion", MySqlDbType.VarChar, 200).Value = nuevoProveedor.Direccion;
                    cmdProv.Parameters.Add("@Telefono", MySqlDbType.VarChar, 50).Value = nuevoProveedor.Telefono;
                    cmdProv.Parameters.Add("@Email", MySqlDbType.VarChar, 100).Value = nuevoProveedor.Email;

                    var newId = Convert.ToInt32(cmdProv.ExecuteScalar());
                    consola.ProveedoresIdProveedor = newId;
                }

                // 2) Actualizar consola
                using var cmdCon = new MySqlCommand(updateConsolaSql, conn, tx);
                cmdCon.Parameters.Add("@Marca", MySqlDbType.VarChar, 100).Value = consola.Marca;
                cmdCon.Parameters.Add("@Modelo", MySqlDbType.VarChar, 100).Value = consola.Modelo;
                cmdCon.Parameters.Add("@Almacenamiento", MySqlDbType.VarChar, 50).Value = consola.Almacenamiento;
                cmdCon.Parameters.Add("@Generacion", MySqlDbType.VarChar, 50).Value = consola.Generacion;
                cmdCon.Parameters.Add("@IncluyeJuegos", MySqlDbType.Bit).Value = consola.IncluyeJuegos;
                cmdCon.Parameters.Add("@Imagen", MySqlDbType.VarChar, 255).Value =
                    string.IsNullOrEmpty(consola.Imagen) ? (object)DBNull.Value : consola.Imagen;
                cmdCon.Parameters.Add("@Precio", MySqlDbType.Decimal).Value = consola.Precio;
                cmdCon.Parameters.Add("@ProveedorId", MySqlDbType.Int32).Value = consola.ProveedoresIdProveedor;
                cmdCon.Parameters.Add("@IdConsola", MySqlDbType.Int32).Value = consola.IdConsola;

                int filas = cmdCon.ExecuteNonQuery();
                if (filas == 0)
                {
                    tx.Rollback();
                    return false;
                }

                tx.Commit();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al editar consola: {ex.Message}");
                return false;
            }
        }


        // ======================= ELIMINAR CONSOLA =======================
        public bool EliminarConsola(int id)
        {
            try
            {
                using (var conn = this.GetConnection())
                {
                    conn.Open();
                    string query = "DELETE FROM Consolas WHERE idConsola = @IdConsola";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdConsola", id);
                        int filasAfectadas = cmd.ExecuteNonQuery();
                        return filasAfectadas > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al eliminar consola: " + ex.Message);
                return false;
            }
        }


        // ======================= INSERTAR CONSOLA CON PROVEEDOR =======================
        public bool InsertarConsola(Consola consola, Proveedor nuevoProveedor = null)
        {
            using (var conn = this.GetConnection())
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        int proveedorId = consola.ProveedoresIdProveedor;

                        // Insertar nuevo proveedor
                        if (nuevoProveedor != null && !string.IsNullOrWhiteSpace(nuevoProveedor.Nombre))
                        {
                            string queryProveedor = @"INSERT INTO Proveedores (Nombre, Direccion, Telefono, Email) 
                                      VALUES (@Nombre, @Direccion, @Telefono, @Email);
                                      SELECT LAST_INSERT_ID();";

                            using (var cmd = new MySqlCommand(queryProveedor, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@Nombre", nuevoProveedor.Nombre);
                                cmd.Parameters.AddWithValue("@Direccion", nuevoProveedor.Direccion ?? "");
                                cmd.Parameters.AddWithValue("@Telefono", nuevoProveedor.Telefono ?? "");
                                cmd.Parameters.AddWithValue("@Email", nuevoProveedor.Email ?? "");
                                proveedorId = Convert.ToInt32(cmd.ExecuteScalar());
                            }
                        }

                        if (proveedorId <= 0)
                        {
                            throw new Exception("ID de proveedor no válido");
                        }

                        string queryConsola = @"INSERT INTO Consolas (Marca, Modelo, Almacenamiento, Generacion, Incluye_Juegos, Imagen, Precio, Proveedores_idProveedor)
                            VALUES (@Marca, @Modelo, @Almacenamiento, @Generacion, @IncluyeJuegos, @Imagen, @Precio, @ProveedorId)";

                        using (var cmd = new MySqlCommand(queryConsola, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@Marca", consola.Marca);
                            cmd.Parameters.AddWithValue("@Modelo", consola.Modelo);
                            cmd.Parameters.AddWithValue("@Almacenamiento", consola.Almacenamiento);
                            cmd.Parameters.AddWithValue("@Generacion", consola.Generacion);
                            cmd.Parameters.AddWithValue("@IncluyeJuegos", consola.IncluyeJuegos);
                            cmd.Parameters.AddWithValue("@Imagen", consola.Imagen);
                            cmd.Parameters.AddWithValue("@Precio", consola.Precio);
                            cmd.Parameters.AddWithValue("@ProveedorId", proveedorId);

                            int rowsAffected = cmd.ExecuteNonQuery();
                            if (rowsAffected == 0)
                                throw new Exception("No se insertó la consola");
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine("Error al insertar consola: " + ex.Message);
                        return false;
                    }
                }
            }
        }








        // ======================= USUARIOS =======================
        // Obtener un usuario por su ID con información general y nivel académico
        public Usuario ObtenerUsuarioPorId(int id)
        {
            Usuario usuario = null;

            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    string query = @"
                SELECT 
                    u.idUSER, u.username, u.password, u.rol,
                    g.nombre, g.apellido, g.sexo, g.Nivel_Academico_idNivel,
                    n.Nivel AS NivelAcademicoNombre,
                    n.Institucion
                FROM user u
                JOIN Generales g ON u.Generales_idGeneral = g.idGeneral
                JOIN Nivel_Academico n ON g.Nivel_Academico_idNivel = n.idNivel
                WHERE u.idUSER = @id";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                usuario = new Usuario
                                {
                                    Id = reader.GetInt32("idUSER"),
                                    Username = reader.GetString("username"),
                                    Password = reader.GetString("password"),
                                    Rol = reader.GetString("rol"),
                                    Nombre = reader.GetString("nombre"),
                                    Apellido = reader.GetString("apellido"),
                                    Sexo = reader.GetString("sexo"),
                                    NivelAcademicoId = reader.GetInt32("Nivel_Academico_idNivel"),
                                    NivelAcademicoNombre = reader.GetString("NivelAcademicoNombre"),
                                    Institucion = reader.IsDBNull(reader.GetOrdinal("Institucion"))
                                                    ? null
                                                    : reader.GetString("Institucion")
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al obtener usuario por ID: " + ex.Message);
            }

            return usuario;
        }






        // Actualizar un usuario
        public bool ActualizarUsuario(Usuario usuario, out string mensajeError)
        {
            mensajeError = string.Empty;

            try
            {
                using var conn = GetConnection();
                conn.Open();

                // 1️⃣ Validar que el nuevo username no exista en otro usuario
                string checkQuery = "SELECT COUNT(*) FROM user WHERE username=@username AND idUSER<>@id";
                using (var checkCmd = new MySqlCommand(checkQuery, conn))
                {
                    checkCmd.Parameters.AddWithValue("@username", usuario.Username);
                    checkCmd.Parameters.AddWithValue("@id", usuario.Id);
                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());
                    if (count > 0)
                    {
                        mensajeError = "Error: el username ya está en uso por otro usuario.";
                        return false;
                    }
                }

                // 2️⃣ Actualizar tabla Generales (nombre, apellido, sexo)
                string updateGenerales = @"
            UPDATE Generales 
            SET nombre=@nombre, apellido=@apellido, sexo=@sexo
            WHERE idGeneral = (SELECT Generales_idGeneral FROM user WHERE idUSER=@id)";
                using (var cmd = new MySqlCommand(updateGenerales, conn))
                {
                    cmd.Parameters.AddWithValue("@nombre", usuario.Nombre);
                    cmd.Parameters.AddWithValue("@apellido", usuario.Apellido);
                    cmd.Parameters.AddWithValue("@sexo", usuario.Sexo);
                    cmd.Parameters.AddWithValue("@id", usuario.Id);
                    cmd.ExecuteNonQuery();
                }

                // 3️⃣ Actualizar Nivel Académico (Nivel e Institución)
                string updateNivel = @"
            UPDATE Nivel_Academico 
            SET Nivel=@nivel, Institucion=@institucion
            WHERE idNivel=@nivelId";
                using (var cmd = new MySqlCommand(updateNivel, conn))
                {
                    cmd.Parameters.AddWithValue("@nivel", usuario.NivelAcademico);
                    cmd.Parameters.AddWithValue("@institucion", usuario.Institucion ?? string.Empty);
                    cmd.Parameters.AddWithValue("@nivelId", usuario.NivelAcademicoId);
                    cmd.ExecuteNonQuery();
                }

                // 4️⃣ Actualizar tabla user (username, password, rol)
                string updateUser = @"
            UPDATE user 
            SET username=@username, password=@password, rol=@rol
            WHERE idUSER=@id";
                using (var cmd = new MySqlCommand(updateUser, conn))
                {
                    cmd.Parameters.AddWithValue("@username", usuario.Username);
                    cmd.Parameters.AddWithValue("@password", usuario.Password); // Considera hashear la contraseña
                    cmd.Parameters.AddWithValue("@rol", usuario.Rol);
                    cmd.Parameters.AddWithValue("@id", usuario.Id);
                    cmd.ExecuteNonQuery();
                }

                return true;
            }
            catch (Exception ex)
            {
                mensajeError = "Error al actualizar usuario: " + ex.Message;
                return false;
            }
        }








        // Eliminar usuario
        // ======================= ELIMINAR USUARIO =======================
        public bool EliminarUsuario(int id)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    using (var tran = conn.BeginTransaction())
                    {
                        try
                        {
                            // 1️⃣ Obtener idGeneral asociado al usuario
                            int idGeneral = 0;
                            string selectGeneral = "SELECT Generales_idGeneral FROM user WHERE idUSER=@id";
                            using (var cmdSelect = new MySqlCommand(selectGeneral, conn, tran))
                            {
                                cmdSelect.Parameters.AddWithValue("@id", id);
                                var result = cmdSelect.ExecuteScalar();
                                if (result != null)
                                    idGeneral = Convert.ToInt32(result);
                            }

                            // 2️⃣ Eliminar usuario
                            string deleteUser = "DELETE FROM user WHERE idUSER=@id";
                            using (var cmdUser = new MySqlCommand(deleteUser, conn, tran))
                            {
                                cmdUser.Parameters.AddWithValue("@id", id);
                                cmdUser.ExecuteNonQuery();
                            }

                            // 3️⃣ Eliminar registro en Generales si no está asociado a ningún otro usuario
                            if (idGeneral > 0)
                            {
                                string deleteGenerales = @"
                            DELETE FROM Generales 
                            WHERE idGeneral=@idGeneral 
                              AND idGeneral NOT IN (SELECT Generales_idGeneral FROM user)";
                                using (var cmdGen = new MySqlCommand(deleteGenerales, conn, tran))
                                {
                                    cmdGen.Parameters.AddWithValue("@idGeneral", idGeneral);
                                    cmdGen.ExecuteNonQuery();
                                }
                            }

                            // 4️⃣ No eliminamos Nivel_Academico ya que son niveles predefinidos

                            tran.Commit();
                            return true;
                        }
                        catch (Exception exTran)
                        {
                            tran.Rollback();
                            Console.WriteLine("Error al eliminar usuario: " + exTran.Message);
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al eliminar usuario: " + ex.Message);
                return false;
            }
        }





        // ======================= OBTENER TODOS LOS USUARIOS =======================
        public List<Usuario> ObtenerUsuarios()
        {
            var lista = new List<Usuario>();

            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    string query = @"
                SELECT u.idUSER, u.username, u.password, u.rol,
                       g.nombre, g.apellido, g.sexo,
                       n.Nivel, n.Institucion
                FROM user u
                JOIN Generales g ON u.Generales_idGeneral = g.idGeneral
                JOIN Nivel_Academico n ON g.Nivel_Academico_idNivel = n.IdNivel";

                    using (var cmd = new MySqlCommand(query, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new Usuario
                            {
                                Id = reader.GetInt32("idUSER"),
                                Username = reader.GetString("username"),
                                Password = reader.GetString("password"),
                                Rol = reader.GetString("rol"),
                                Nombre = reader.GetString("nombre"),
                                Apellido = reader.GetString("apellido"),
                                Sexo = reader.GetString("sexo"),
                                NivelAcademico = reader.GetString("Nivel"),
                                Institucion = reader.IsDBNull(reader.GetOrdinal("Institucion"))
                                                ? string.Empty
                                                : reader.GetString("Institucion")
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al obtener usuarios: " + ex.Message);
            }

            return lista;
        }





        

    }
}
