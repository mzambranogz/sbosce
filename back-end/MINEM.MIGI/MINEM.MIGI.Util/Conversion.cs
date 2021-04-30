using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MINEM.MIGI.Util
{
    public class Conversion
    {
        public static int LetrasANumero(string letras)
        {
            int numero = 0;
            if ("Y" == letras) numero = -1; //Indica que puede continuar con la siguiente palabra
            else if ("CERO" == letras) numero = 0;
            else if ("UN" == letras) numero = 1;
            else if ("UNO" == letras) numero = 1;
            else if ("DOS" == letras) numero = 2;
            else if ("TRES" == letras) numero = 3;
            else if ("CUATRO" == letras) numero = 4;
            else if ("CINCO" == letras) numero = 5;
            else if ("SEIS" == letras) numero = 6;
            else if ("SIETE" == letras) numero = 7;
            else if ("OCHO" == letras) numero = 8;
            else if ("NUEVE" == letras) numero = 9;
            else if ("DIEZ" == letras) numero = 10;
            else if ("ONCE" == letras) numero = 11;
            else if ("DOCE" == letras) numero = 12;
            else if ("TRECE" == letras) numero = 13;
            else if ("CATORCE" == letras) numero = 14;
            else if ("QUINCE" == letras) numero = 15;
            else if (letras.IndexOf("DIECI") != -1)
            {
                string num_cadena = letras.Substring(5);
                int num = LetrasANumero(num_cadena);
                numero = num + 10;
            }
            else if ("VEINTE" == letras) numero = 20;
            else if (letras.IndexOf("VEINTI") != -1)
            {
                string num_cadena = letras.Substring(6);
                int num = LetrasANumero(num_cadena);
                numero = num + 20;
            }
            else if ("TREINTA" == letras) numero = 30;
            else if ("CUARENTA" == letras) numero = 40;
            else if ("CINCUENTA" == letras) numero = 50;
            else if ("SESENTA" == letras) numero = 60;
            else if ("SETENTA" == letras) numero = 70;
            else if ("OCHENTA" == letras) numero = 80;
            else if ("NOVENTA" == letras) numero = 90;
            else if ("CIEN" == letras) numero = 100;
            else if ("CIENTO" == letras) numero = 100;
            else if ("QUINIENTOS" == letras) numero = 500;
            else if ("SETECIENTOS" == letras) numero = 700;
            else if ("NOVECIENTOS" == letras) numero = 900;
            else if (letras.IndexOf("CIENTOS") != -1)
            {
                int i = letras.IndexOf("CIENTOS");
                string num_cadena = letras.Substring(0, i);
                int num = LetrasANumero(num_cadena);
                numero = num * 100;
            }
            else if ("MIL" == letras) numero = 1000;
            else if ("MILLON" == letras) numero = 1000000;
            else if ("MILLONES" == letras) numero = 1000000;
            return numero;
        }
    }
}
