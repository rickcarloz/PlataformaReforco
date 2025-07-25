using System.ComponentModel;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Web;

namespace Project.BLL.Common
{
    public static class Utility
    {
        public static string ObjToQueryString(this object obj, bool urlEncode = true, string concatString = "&")
        {

            return obj == null ? "" : string.Join(concatString, JsonSerializer.Deserialize<IDictionary<string, object>>(JsonSerializer.Serialize(obj))
                 .Where(x => x.Value != null)
                .Select(x => HttpUtility.UrlEncode(x.Key) + "=" + (urlEncode ? HttpUtility.UrlEncode(x.Value.ToString()) : x.Value.ToString())));
        }


        public static string ModelToQueryString(object obj, bool urlEncode = true, string concatString = "&")
        {
            return obj == null ? "" : string.Join(concatString, JsonSerializer.Deserialize<IDictionary<string, object>>(JsonSerializer.Serialize(obj))
                .Where(x => x.Value != null)
                .Select(x => HttpUtility.UrlEncode(x.Key) + "=" + (urlEncode ? HttpUtility.UrlEncode(x.Value.ToString()) : x.Value.ToString())));
        }

        public static string ObjToQuerySQL(this object obj)
        {
            if (obj == null)
            {
                return "";
            }

            var query = new List<QueryMaker>();
            foreach (var x in JsonSerializer.Deserialize<IDictionary<string, string>>(JsonSerializer.Serialize(obj)).Where(x => !string.IsNullOrEmpty(x.Value)))
            {
                query.Add(new QueryMaker()
                {
                    key = x.Value.ToString().Contains("[OR]") ? " OR " : " AND ",
                    value = x.Key + (x.Value.ToString().Contains("[FX]") ? x.Value.ToString().Replace("[FX]", "").Replace("[OR]", "") : " = '" + x.Value + "'")
                });
            }

            var where = new List<string>();
            foreach (var x in query.Where(x => x.key.Contains("OR")).GroupBy(x => x.key))
            {
                where.Add("(" + string.Join(x.Key, x.Select(x => x.value)) + ")");
            }

            foreach (var x in query.Where(x => x.key.Contains("AND")))
            {
                where.Add("(" + string.Join(x.key, x.value) + ")");
            }

            return string.Join(" AND ", where.ToList());

        }




        public static string AdicionarZeros(this string texto, int zeros)
        {
            int.TryParse(texto, out int Valor);
            var stringNum = new string('0', zeros);
            texto = Valor.ToString(stringNum);
            return texto;
        }

        public static string AdicionarZerosEsquerda(this string texto, int zeros)
        {
            var result = texto.PadLeft(zeros, '0');
            return result;
        }


        public static string OnlyNumbers(this string texto)
        {
            Regex regex = new Regex(@"\d+");
            string apenasNumeros = string.Join("", regex.Matches(texto));

            return apenasNumeros;
        }


        public static string AsCnpj(this string value)
        {
            var numeros = OnlyNumbers(value);
            MaskedTextProvider mtpCnpj = new MaskedTextProvider(@"00\.000\.000/0000-00");

            if (!string.IsNullOrEmpty(numeros) && numeros.Length == 14)
            {
                mtpCnpj.Set(numeros);
                return mtpCnpj.ToString();
            }

            return value;
        }

        public static string AsCpf(this string value)
        {
            var numeros = OnlyNumbers(value);
            MaskedTextProvider mtpCpf = new MaskedTextProvider(@"000\.000\.000-00");
            if (!string.IsNullOrEmpty(numeros) && numeros.Length == 11)
            {
                mtpCpf.Set(numeros);
                return mtpCpf.ToString();
            }

            return value;
        }


        public static bool TryCast<T>(this object obj, out T result)
        {
            if (obj is T)
            {
                result = (T)obj;
                return true;
            }

            result = default(T);
            return false;
        }

    }

    public class QueryMaker
    {
        public string key { get; set; }
        public string value { get; set; }
    }
}
