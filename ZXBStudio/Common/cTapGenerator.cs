
// Generador de .TAP v0.1
//
// Antonio Tamairón - 04/05/2022
// @hash6iron / hash6iron@gmail.com
//
// Rutina para generar un fichero .TAP de BYTES o SCREEN (no se genera el cargador de arranque) a partir de una serie de parámetros:
// - Bloque de datos (bytes en RAW)
// - Dirección de almacenamiento en memoria del ZX Spectrum
// - Nombre del fichero (max. 10 caracteres)
//
// Uso: 
//   generateTAP(tFileTap file)
//
//   donde, tFileTap es un tipo estructural.
//   * data: Bloque de bytes a insertar en el TAP
//   * startAddress: Dirección a partir de la cual se carga el bloque de bytes
//   * blockName: Nombre para el bloque de datos (tipico nombre que aparece en la carga al lado de byte: xxxxxxxxxx ). Max. 10 caracteres
//   * blockSize: Tamaño del bloque de bytes a insertar en el TAP 
//
// Primera versión: v0.1
//
// Historial de bugs


using System;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace ZXBasicStudio.Common
{
    public class cTapGenerator
    {

        // Generamos la estructura de un .tap
        public struct tTapFile
        {
            public string blockName;
            public int blockSize;
            public int startAddress;
            public byte[] data;
        }

        //Generamos el constructor
        cTapGenerator()
        {
        }

        // Metodos
        public byte[] createTap(tTapFile fileTap) 
        {
            // Este metodo crea todo el bloque de bytes que compone el TAP
            // cabecera + datos

            // Inicializamos variables y arrays necesarios
            tTapFile fileTemp = new tTapFile();
            byte[] codeBlockOut = new byte[1] { 0x00 };
            fileTemp = fileTap;

            // Comprobamos que el bloque de datos no está vacio
            if (fileTap.data is not null)
            {
                // Comprobamos que existe un nombre para el TAP
                if (fileTap.blockName != "")
                {
                    // El nombre de bloque debe estar preparado. Con un total de longitud ocupada de 10 caracteres
                    if (fileTap.blockName.Length <= 10)
                    {
                        // Rellenamos con espacios hasta cumplir 10 caracteres
                        string blockNameEnd = new string(' ', 10 - fileTap.blockName.Length);
                        string blockNameTmp = fileTap.blockName + blockNameEnd;
                        fileTap.blockName = blockNameTmp;
                    }
                    else 
                    {
                        // Entonces. Acortamos el nombre
                        fileTap.blockName = fileTap.blockName.Substring(0, 9);
                    }

                    // Creamos el TAP
                    codeBlockOut = makeTap(fileTap);
                    Debug.WriteLine("TapGenerator: TAP byte block generate successfully");
                }
                else
                {
                    // Error. No se ha asignado un nombre al bloque
                    Debug.WriteLine("TapGenerator: rawByteCode is null");
                }
            }
            else
            {
                // Error. No se genera nada.
                Debug.WriteLine("TapGenerator: data TAP block is null. Stop the process.");
            }

            return codeBlockOut;
        }

        private byte[] combine(byte[] array1, byte[] array2)
        {
            // Este procedimiento combina dos arrays de bytes

            byte[] arrayOut = new byte[0];
            int pos = array1.Length;
            Array.Resize(ref array1, array1.Length + array2.Length);

            foreach (byte b in array2) 
            {
                array1[pos] = b;
                pos++;
            }

            arrayOut = array1;
            return arrayOut;
        }

        private byte[] makeTap(tTapFile fileTap)
        {
            byte[] codeBlockOut = new byte[4] { 0x13, 0x00, 0x00, 0x03 };

            //
            // Insertamos el nombre del bloque
            byte[] blockName_Hex = new byte[0];
            blockName_Hex = Encoding.UTF8.GetBytes(fileTap.blockName);
            codeBlockOut = combine(codeBlockOut, blockName_Hex);

            // Insertamos la longitud del bloque (2 bytes)
            UInt16 blockSize = Convert.ToUInt16(fileTap.blockSize);
            byte[] blockSize_hex = BitConverter.GetBytes(fileTap.blockSize);
            codeBlockOut = combine(codeBlockOut, blockSize_hex[0..2]);

            // Insertamos la direccion de inicio del bloque (2 bytes)
            UInt16 startAddress = Convert.ToUInt16(fileTap.startAddress);
            byte[] startAddress_hex = BitConverter.GetBytes(fileTap.startAddress);
            codeBlockOut = combine(codeBlockOut, startAddress_hex[0..2]);

            // Insertamos 32768. Para CODE y SCREEN
            byte[] fix = new byte[2] {0x00,0x80};
            codeBlockOut = combine(codeBlockOut, fix);

            // Calculamos el checksum del bloque menos el primer byte (1 byte)
            UInt16 chkHeader = Convert.ToUInt16(checksum(codeBlockOut[2..codeBlockOut.Length]));
            byte[] chkHeader_hex = BitConverter.GetBytes(chkHeader);
            codeBlockOut = combine(codeBlockOut, chkHeader_hex[0..1]);

            // Insertamos la longitud del bloque + 2 bytes. Posicion 21
            UInt16 blockSize2 = Convert.ToUInt16(fileTap.blockSize + 2);
            byte[] blockSize2_hex = BitConverter.GetBytes(blockSize2);
            codeBlockOut = combine(codeBlockOut, blockSize2_hex[0..2]);
            // Con este procedimiento cogemos el bloque de bytes y generamos el .tap1

            // Insertamos el fin de la cabecera - posicion 23
            byte[] endBlock = new byte[1] {0xFF};
            codeBlockOut = combine(codeBlockOut, endBlock);

            // Ahora insertamos el bloque de datos
            codeBlockOut = combine(codeBlockOut, fileTap.data);

            // Ahora insertamos el checksum del bloque de datos (1 byte) contando con 0xFF al inicio (posicion 23)
            UInt16 chkDataBlock = Convert.ToUInt16(checksum(codeBlockOut[23..(fileTap.data.Length+24)]));
            byte[] chkDataBlock_hex = BitConverter.GetBytes(chkDataBlock);
            codeBlockOut = combine(codeBlockOut, chkDataBlock_hex[0..1]);

            // Devolvemos el bloque de bytes TAP generado
            return codeBlockOut;
        }

        public void saveToTapFile(byte[] tapCodeBlock, string path)
        {
            // Procedimiento para almacenar el bloque de bytes en un fichero
            // formato binario.

            // Ejemplo de path:
            // @"c:\temp\MyTest.txt";

            if (!File.Exists(path))
            {
                // Creamos un fichero con formato binario que contiene
                // todo el bloque de bytes del TAP, generado.
                using (BinaryWriter binFile = new BinaryWriter(File.Open(path, FileMode.Create)))
                {
                    binFile.Write(tapCodeBlock);
                }
            }
            else
            {
                // Error. Ya existe el fichero
                Debug.WriteLine("TapGenerator: File already exists.");
            }
        }

        private int checksum(byte[] data)
        { 
            // Calculamos el checksum. Tipico XOR
            int checksum = 0;
            for (int n = 0; n < data.Length; n++)
            {
                // Recorremos todo el bloque calculando el checksum XOR
                checksum = checksum ^ data[n];
            }
            return checksum;
        }

    }
}
