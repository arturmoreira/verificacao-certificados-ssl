using System;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using VerificacaoCertificadosSSL.Properties;

namespace VerificacaoCertificadosSSL
{
    class Program
    {
        static void Main(string[] args)
        {
            //"https://google.com"

            if (Settings.Default.AntecedenciaDiasAviso < Settings.Default.AntecedenciaDiasErro)
            {
                Console.WriteLine("*******************************************************************************************************************");
                Console.WriteLine("*** O número de dias de antecedência do aviso deve ser maior ou igual ao número de dias de antecedência do erro ***");
                Console.WriteLine("*******************************************************************************************************************");
                Console.WriteLine();
            }

            foreach (var urlParaVerificar in Settings.Default.ListaUrl)
            {
                Console.WriteLine(urlParaVerificar.Replace("https://", string.Empty));
                Console.Write("  ");

                var validadeCertificado = RetornarValidadeCertificadoSsl(urlParaVerificar);

                if (validadeCertificado.HasValue)
                {
                    var diasAteExpiracaoCertificado = (int)validadeCertificado.Value.Subtract(DateTime.Now).TotalDays;

                    Console.ForegroundColor = CorMensagem(diasAteExpiracaoCertificado);
                    Console.Write(validadeCertificado);
                    Console.Write(" ");
                    Console.WriteLine(MensagemDiasAteExpiracao(diasAteExpiracaoCertificado));
                }
                else
                {
                    // Validade desconhecida.
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine("Não foi possível obter a validade do certificado");
                }

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine();
            }

            //Console.WriteLine(RetornarValidadeCertificadoSsl("https://google.com"));
            //Console.WriteLine(RetornarValidadeCertificadoSsl("https://arturmoreira.eu"));
            //Console.WriteLine(RetornarValidadeCertificadoSsl("https://www.arturmoreira.eu"));
            //dipromed.dnsalias.com
        }

        static DateTime? RetornarValidadeCertificadoSsl(string url)
        {
            HttpWebResponse response = null;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            X509Certificate cert2 = null;

            try
            {
                response = (HttpWebResponse)request.GetResponse();
                X509Certificate cert = request.ServicePoint.Certificate;
                cert2 = new X509Certificate2(cert);
            }
            catch
            {
                X509Certificate cert = request.ServicePoint.Certificate;
                if (cert != null)
                {
                    cert2 = new X509Certificate2(cert);
                }
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                    response.Dispose();
                }
            }

            if (cert2 != null)
            {
                string certExpirationDate = cert2.GetExpirationDateString();
                return DateTime.Parse(certExpirationDate);
            }

            return null;
        }

        static ConsoleColor CorMensagem(int antecedenciaDias)
        {
            if (antecedenciaDias <= Settings.Default.AntecedenciaDiasErro)
                return ConsoleColor.Red;

            if (antecedenciaDias <= Settings.Default.AntecedenciaDiasAviso)
                return ConsoleColor.Yellow;

            return ConsoleColor.Green;
        }

        static string MensagemDiasAteExpiracao(int dias)
        {
            if (dias < 0)
                return "*** EXPIRADO ***";

            if (dias == 0)
                return "(expira hoje)";

            if (dias == 1)
                return "(falta 1 dia)";

            return $"(faltam {dias} dias)";
        }
    }
}