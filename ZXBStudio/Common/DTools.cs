using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZXBasicStudio.Common
{
    /// <summary>
    /// Common tools for data conversion, from DuefectuTools
    /// </summary>
    public static class DTools
    {
        /// <summary>
        /// Fecha utilizada como sustituto de null en las operaciones con DateTime
        /// </summary>
        public static DateTime FechaNula = new DateTime(1900, 1, 1);

        #region Conversion de tipos de datos

        /// <summary>
        /// Intenta convertir un objeto a una fecha, devolviendo
        /// una cadena vacia ("") si no es una fecha válida.
        /// Uso: string fecha = mifecha.ToDate();
        /// </summary>
        /// <param name="valor">Objeto a convertir</param>
        /// <param name="formato">Formato de salida opcional. Por defecto "dd/MM/yyyy"</param>
        /// <param name="convertirUSA">Si es true (false por defecto), convierte el formato desde un tipo USA, donde primero se coloca el mes y
        /// después el día. Es decir, el formato de entrada es MM/dd/yyyy</param>
        /// <returns>Cadena con la fecha formateada o una cadena vacia ("") si
        /// no se ha podido convertir la fecha</returns>
        public static string ToDateString(this object valor, string formato = "dd/MM/yyyy", bool convertirUSA = false)
        {
            try
            {
                string frmt = formato;
                if (convertirUSA)
                {
                    if (frmt.StartsWith("dd/"))
                    {
                        frmt = frmt.Replace("dd/", "xx/").Replace("/MM/", "/zz/");
                        frmt = frmt.Replace("xx/", "MM/").Replace("/zz/", "/dd/");
                    }
                }
                DateTime fecha = DateTime.MinValue;
                if (DateTime.TryParse(valor.ToString(), out fecha))
                {
                    return fecha.ToString(frmt);
                }
            }
            catch { }
            return "";
        }

        /// <summary>
        /// Intenta convertir un objeto a una fecha, devolviendo
        /// 1/1/1900 si no es una fecha válida.
        /// Uso: string fecha = mifecha.ToDate();
        /// </summary>
        /// <param name="valor">Objeto a convertir</param>
        /// <returns>Datetime con la fecha formateada o 1/1/1900 (FechaNula) si
        /// no se ha podido convertir la fecha</returns>
        public static DateTime ToDate(this object valor)
        {
            try
            {
                DateTime fecha = DateTime.MinValue;
                if (DateTime.TryParse(valor.ToString(), out fecha))
                {
                    if (fecha < FechaNula)
                    {
                        return FechaNula;
                    }
                    return fecha;
                }
            }
            catch { }
            return FechaNula;
        }

        /// <summary>
        /// Intenta convertir un objeto a una hora, devolviendo
        /// 1/1/1900 00:00:00 si no es una fecha válida. 
        /// La fecha siempre es 1/1/1900 más la hora con resolución de segundos, por ejemplo
        /// "23/2/2019 14:25:42.2563" se transforma a "1/1/1900 14:25:42"
        /// La parte de fecha se destruye siempre!!!
        /// Uso: string hora = mifecha.ToHour();
        /// </summary>
        /// <param name="valor">Objeto a convertir</param>
        /// <returns>Datetime con la fecha formateada o 1/1/1900 (FechaNula) si
        /// no se ha podido convertir la fecha</returns>
        public static DateTime ToHour(this object valor)
        {
            try
            {
                var fecha = valor.ToDate();
                var hour = FechaNula.AddHours(fecha.Hour).AddMinutes(fecha.Minute).AddSeconds(fecha.Second);
                return hour;
            }
            catch { }
            return FechaNula;
        }

        /// <summary>
        /// Intenta convertir un objeto a entero, devolviendo
        /// 0 si no se puede convertir
        /// Uso: int entero = miObjeto.ToInteger();
        /// </summary>
        /// <param name="valor">Objeto a convertir</param>
        /// <returns>Entero con el valor de la conversión o 0 si hay un error</returns>
        public static int ToInteger(this object valor)
        {
            try
            {
                if (valor == null)
                {
                    return 0;
                }
                int v = 0;
                if (int.TryParse(valor.ToString(), out v))
                {
                    return v;
                }
            }
            catch { }
            return 0;
        }

        /// <summary>
        /// Intenta convertir un objeto a decimal, devolviendo
        /// 0 si no se puede convertir
        /// Uso: int entero = miObjeto.ToDecimal();
        /// </summary>
        /// <param name="valor">Objeto a convertir</param>
        /// <returns>Decimal con el valor de la conversión o 0 si hay un error</returns>
        public static decimal ToDecimal(this object valor)
        {
            try
            {
                decimal v = 0;
                if (decimal.TryParse(valor.ToString(), out v))
                {
                    return v;
                }
            }
            catch { }
            return 0;
        }

        /// <summary>
        /// Intenta convertir un objeto a decimal, devolviendo
        /// 0 si no se puede convertir
        /// Los puntos (.) del objeto se eliminan y las comas (,) se 
        /// cambian por puntos (.) -> 1.234,56 = 1234.56 antes de convertir
        /// Uso: int entero = miObjeto.ToDecimal();
        /// </summary>
        /// <param name="valor">Objeto a convertir</param>
        /// <returns>Decimal con el valor de la conversión o 0 si hay un error</returns>
        public static decimal ToDecimalConPuntos(this object valor, string separadorDeMiles = ".", string separadorDecimal = ",")
        {
            try
            {
                decimal v = 0;
                string vS = valor.ToStringNoNull();
                var tieneMiles = vS.Contains(separadorDeMiles);
                var tieneDecimal = vS.Contains(separadorDecimal);
                if (tieneMiles && tieneDecimal)
                {
                    vS = vS.Replace(separadorDeMiles, "");
                }
                else if (tieneMiles)
                {
                    vS = vS.Replace(separadorDeMiles, separadorDecimal);
                }
                if (decimal.TryParse(vS.ToString(), out v))
                {
                    return v;
                }
            }
            catch { }
            return 0;
        }

        /// <summary>
        /// Intenta convertir un objeto a byte, devolviendo
        /// 0 si no se puede convertir
        /// Uso: byte entero = miObjeto.ToByte();
        /// </summary>
        /// <param name="valor">Objeto a convertir</param>
        /// <returns>Byte con el valor de la conversión o 0 si hay un error</returns>
        public static byte ToByte(this object valor)
        {
            try
            {
                if (valor == null)
                {
                    return 0;
                }
                int v = 0;
                if (int.TryParse(valor.ToString(), out v))
                {
                    return (byte)v;
                }
            }
            catch { }
            return 0;
        }

        /// <summary>
        /// Intenta convertir un objeto a booleano, devolviendo
        /// false si no se puede convertir
        /// Uso: bool valor = miObjeto.ToBoolean();
        /// </summary>
        /// <param name="valor">Objeto a convertir</param>
        /// <returns>bool con el valor de la conversión o false si hay un error</returns>
        public static bool ToBoolean(this object valor)
        {
            try
            {
                bool v = false;
                if (bool.TryParse(valor.ToString(), out v))
                {
                    return v;
                }
            }
            catch { }
            return false;
        }

        /// <summary>
        /// Intenta convertir un objeto a string, devolviendo
        /// "" si no se puede convertir
        /// Uso: string cadena = miObjeto.ToString();
        /// </summary>
        /// <param name="valor">Objeto a convertir</param>
        /// <returns>Cadena con el valor de la conversión o "" si hay un error</returns>
        public static string ToStringNoNull(this object valor)
        {
            try
            {
                if (valor == null)
                {
                    return "";
                }
                else
                {
                    return valor.ToString();
                }
            }
            catch { }
            return "";
        }

        /// <summary>
        /// Intenta convertir un objeto a Guid, devolviendo
        /// Guid.Empty si no se puede convertir
        /// Uso: Guid g = miObjeto.ToGuid();
        /// </summary>
        /// <param name="valor">Objeto a convertir</param>
        /// <returns>Guid con el valor de la conversión o Guid.Empty si hay un error</returns>
        public static Guid ToGuid(this object valor)
        {
            try
            {
                Guid g = Guid.Empty;
                if (valor == null)
                {
                    return g;
                }
                Guid.TryParse(valor.ToString(), out g);
                return g;
            }
            catch { }
            return Guid.Empty;
        }

        #endregion


        #region Serialización

        /// <summary>
        /// Serializa un objeto
        /// </summary>
        /// <param name="datos">Objeto a serializar</param>
        /// <returns>String en formato Json</returns>
        public static string Serializar(this object datos)
        {
            try
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(datos);
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// Deserializa un objeto
        /// </summary>
        /// <typeparam name="T">Tipo de dato esperado</typeparam>
        /// <param name="datos">datos a deserializar</param>
        /// <returns>Objeto deserializado</returns>
        public static T Deserializar<T>(this object datos)
        {
            try
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(datos.ToStringNoNull());
            }
            catch
            {
                return default(T);
            }
        }

        /// <summary>
        /// Clona un objeto serializandolo y deserializandolo
        /// </summary>
        /// <typeparam name="T">Tipo de objeto a clonar</typeparam>
        /// <param name="objetoOriginal">Objeto a clonar</param>
        /// <returns>Objeto clonado</returns>
        public static T Clonar<T>(this object objetoOriginal)
        {
            try
            {
                string ser = objetoOriginal.Serializar();
                T nuevo = ser.Deserializar<T>();
                return nuevo;
            }
            catch
            {
                return default(T);
            }
        }

        #endregion


        #region Tratamiento de datos varios

        /// <summary>
        /// Combina partes de una URL para formar una dirección completa de forma similar
        /// a Path.Combine
        /// </summary>
        /// <param name="partes">Partes de la url</param>
        /// <returns>Url montada</returns>
        public static string URLCombine(params string[] partes)
        {
            try
            {
                var sb = new StringBuilder();
                foreach (var parte in partes)
                {
                    sb.Append(parte.TrimStart('/').TrimEnd('/') + '/');
                }
                return sb.ToString();
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// Limita el tamaño de una cadena de texto, cortando lo que excede el tamaño máximo
        /// </summary>
        /// <param name="texto">Texto a limitar</param>
        /// <param name="tamañoMaximo">Tamaño máximo del texto</param>
        /// <returns>Texto limitado</returns>
        public static string LimitarTamaño(this string texto, int tamañoMaximo)
        {
            var txt = texto;
            if (txt.Length > tamañoMaximo)
            {
                txt = txt.Substring(0, tamañoMaximo);
            }
            return txt;
        }

        #endregion

    }
}
