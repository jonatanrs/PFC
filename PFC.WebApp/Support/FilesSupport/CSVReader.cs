using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PFC.WebApp.Controllers;
using static Microsoft.AspNetCore.WebSockets.Internal.Constants;

namespace PFC.WebApp.Support.FilesSupport
{
    /// <summary>
    /// Clase que parsea un fichero CSV
    /// </summary>
    /// <seealso cref="System.Collections.Generic.IEnumerable{System.String[]}" />
    /// <seealso cref="System.IDisposable" />
    public class CSVReader : IEnumerable<string[]>, IDisposable
    {
        private Stream _stream;
        private readonly Encoding _encoding;
        private readonly char _separator;
        private readonly bool _stringDelimiter;
        private readonly bool _firstLineAsHeader;

        public string[] Headers { get; private set; }

        /// <summary>
        /// Inicializa una nueva instancia de CSVImporter.
        /// </summary>
        /// <param name="stream">Stream con los datos a importar.</param>
        /// <param name="encoding">The encoding.</param>
        /// <param name="separator">el caracter separador.</param>
        /// <param name="stringDelimiter">Si se establece a verdadero, indica que se usará el carácter doble comilla como delimitador de strings.</param>
        /// <param name="firstLineAsHeader">if set to <c>true</c> [first line as header].</param>
        /// <exception cref="NotSupportedException">El stream no soporta posicionamiento</exception>
        public CSVReader(Stream stream, char separator = ';', Encoding encoding = null, bool stringDelimiter = false, bool firstLineAsHeader = false)
        {
            if (!stream.CanSeek)
                throw new NotSupportedException("El stream no soporta posicionamiento");

            _stream = stream;
            _encoding = encoding ?? Encoding.Default;            
            _separator = separator;
            _stringDelimiter = stringDelimiter;
            _firstLineAsHeader = firstLineAsHeader;

            if (firstLineAsHeader)
            {
                _stream.Position = 0;
                using (var streamReader = new StreamReader(_stream, _encoding, true, 4096, true))
                {
                    Headers = lineParse(streamReader);
                }
            }
        }

        /// <summary>
        /// Libera el stream pasado en el constructor.
        /// </summary>
        public void Dispose()
        {
            _stream.Dispose();
        }

        private string[] lineParse(StreamReader reader)
        {
            return _stringDelimiter
                ? complexLineParse(reader)
                : simpleLineParse(reader);
        }

        /// <summary>
        /// Parsea CSVs cuyas cadenas de texto no están delimitados por lo que cada línea del fichero debe ser una línea CSV.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        private string[] simpleLineParse(StreamReader reader)
        {
            return reader.ReadLine().Split(_separator);
        }

        /// <summary>
        /// Parsea CSVs que contienen cadenas delimitadas por el caracter de dobles comillas.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        private string[] complexLineParse(StreamReader reader)
        {
            List<string> fields = new List<string>();
            StringBuilder current_field = new StringBuilder();
            int comillasCount = 0;
            while (!reader.EndOfStream)
            {
                string linea = reader.ReadLine();
                for (int n = 0; n < linea.Length; n++)
                {
                    if (linea[n] == '\"')
                    {
                        comillasCount++;
                    }

                    if ((linea[n] == _separator) && ((comillasCount % 2) == 0))
                    {
                        if (comillasCount > 0)
                        {
                            //Eliminar comillas de inicio y final
                            fields.Add(current_field.ToString().Substring(1, current_field.Length - 2));
                        }
                        else
                            fields.Add(current_field.ToString());

                        current_field = new StringBuilder();
                        comillasCount = 0;
                    }
                    else
                        current_field.Append(linea[n]);
                }
                if ((comillasCount % 2) == 0)
                {
                    fields.Add(current_field.ToString());
                    return fields.ToArray();
                }
                current_field.AppendLine();
            }
            return fields.ToArray();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<string[]> GetEnumerator()
        {
            using (var streamReader = new StreamReader(_stream, _encoding, true, 1024, true))
            {
                _stream.Position = 0;

                if (_firstLineAsHeader)
                    lineParse(streamReader);

                while (!streamReader.EndOfStream)
                    yield return lineParse(streamReader);
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
