using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NegocioDados
{
    public class NegocioLog
    {

        public NegocioLog()
        { 
            string connectionString = "Data Source=187.84.228.60;Initial Catalog=dbMobLinkBoletos;Persist Security Info=True;User ID=ws_patio;Password=Studio55#;Connection Timeout=360";
            ConnectionFactory.connectionString = connectionString;
        }

        /*public static void UpdateLinhaDigitavel(string linha)
        {
            StringBuilder SQL;


            SQL = new StringBuilder();
            //id = 0;
            try
            {
                if (bol != null)
                {
                    SQL.AppendFormat(@"UPDATE dbo.tb_boletos SET linha_digitavel='{0}', 
                                   boleto_numeroDocumento='{1}',
                                   cedente_cpfCnpj ='{2}',
                                   cedente_nome = '{3}',
                                   id_benificiario_final = {4}
                                   where id_boleto={1}", linha, id, bol.cedente_cpfCnpj, bol.cedente_nome, bol.id_beneficiario_final);

                }
                else
                {
                    SQL.AppendFormat(@"UPDATE dbo.tb_boletos SET linha_digitavel='{0}', 
                                   boleto_numeroDocumento='{1}'
                                   where id_boleto={1}", linha, id);

                }

                if (isdev)
                {
                    string teste = SQL.ToString().Replace("dbo.", "dbmoblinkboletosdev.dbo.");
                    SQL.Clear();
                    SQL.Append(teste);
                }

                ConnectionFactory.Executar(SQL.ToString());

            }
            catch (Exception) { throw; }
        }*/
    }
}
