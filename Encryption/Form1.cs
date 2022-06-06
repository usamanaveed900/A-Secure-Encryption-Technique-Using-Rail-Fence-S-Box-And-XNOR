using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.IO;
using System.Threading;
using System.Net;

namespace Encryption
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        string plainText = null;
        string ABC = null;
        static string encryptedKey = null;
        static string[] Scopes = { DriveService.Scope.Drive };
        static string ApplicationName = "ENCRYPTION_UPLOAD_FILE";
        string loadFileName = null;

        public static void writeOnFile(string data,string name)
        {
            string path = @"";
            for (int i = 1; i <= 2; i++)
            {
                string Filename = name+"Encrypted" + i + ".txt";
                var newpath = path + Filename; 
                using (StreamWriter sw = System.IO.File.AppendText(newpath))
                {
                    sw.WriteLine(data);
                }
            }
        }

        public static void clearFileContent(string name)
        {
            for (int i = 1; i <= 2; i++)
            {
                System.IO.File.WriteAllText(name+@"Encrypted" + i + ".txt", string.Empty);
            }
        }

        public static void write(string data,string name)
        {
            string path = @"";
            string Filename = name+"Encrypted.txt";
            var newpath = path + Filename; 
            using (StreamWriter sw = System.IO.File.AppendText(newpath))
            {
                sw.WriteLine(data);
            }

        }

        public static void writeDecoded(string data,string name)
        {
            string path = @"";
            string Filename = name+"Decoded.txt";
            var newpath = path + Filename;
            using (StreamWriter sw = System.IO.File.AppendText(newpath))
            {
                sw.WriteLine(data);
            }

        }

        public static void removeFileContentDecoded(string name)
        {
            System.IO.File.WriteAllText(name+@"Decoded.txt", string.Empty);
        }

        public static void removeFileContent(string name)
        {
            System.IO.File.WriteAllText(name+@"Encrypted.txt", string.Empty);
        }

        public static string convertToASCII(string unicodeString)
        {
            string ASCIIcode = null;
            var utf8bytes = Encoding.UTF8.GetBytes(unicodeString);
            var win1252Bytes = Encoding.Convert(Encoding.UTF8, Encoding.ASCII, utf8bytes);
            foreach (var item in win1252Bytes)
            {
                ASCIIcode += item + " ";
            }
            return ASCIIcode;
        }

        public static string ASCIIToBinary(string str)
        {
            StringBuilder sb = new StringBuilder();

            foreach (char c in str.ToCharArray())
            {
                sb.Append(Convert.ToString(c, 2).PadLeft(8, '0'));
            }
            return sb.ToString();
        }

        public static Byte[] GetBytesFromBinaryString(String binary)
        {
            var list = new List<Byte>();

            for (int i = 0; i < binary.Length; i += 8)
            {
                String t = binary.Substring(i, 8);

                list.Add(Convert.ToByte(t, 2));
            }

            return list.ToArray();
        }

        public static string randomBinary()
        {
            Random rnd = new Random();
            string rand = rnd.Next(0, 15).ToString();
            string binary = Convert.ToString(Convert.ToInt32(rand, 10), 2);
            if (binary.Length == 1)
            {
                binary = "000" + binary;
            }
            else if (binary.Length == 2)
            {
                binary = "00" + binary;
            }
            else if (binary.Length == 3)
            {
                binary = "0" + binary;
            }
            else
            { }
            return binary;
        }

        public static string binaryComplement(string binary)
        {
            char[] temp = new char[4];
            for (int i = 0; i < temp.Length; i++)
            {
                temp[i] = binary[i];
            }

            for (int i = 0; i < temp.Length; i++)
            {
                if (temp[i] == '0')
                {
                    temp[i] = '1';
                }
                else
                {
                    temp[i] = '0';
                }
            }
            string s = new string(temp);
            return s;
        }

        public static string S_box(string temp)
        {
            char[,] Box = new char[4, 4] { { 'A', 'C', 'T', 'G' }, { 'T', 'A', 'G', 'C' }, { 'G', 'T', 'C', 'A' }, { 'C', 'G', 'A', 'T' } };
            string first = temp.Substring(0, 2);
            string second = temp.Substring(2, 2);
            string firstIndex = Convert.ToString(Convert.ToInt32(first, 2), 10);
            string secondIndex = Convert.ToString(Convert.ToInt32(second, 2), 10);
            return (temp = Box[int.Parse(firstIndex), int.Parse(secondIndex)].ToString());
        }

        public static string XNOR(string key, string Number)
        {
            char[,] Box = new char[5, 5] { { '0', 'T', 'A', 'C', 'G' }, { 'T', 'T', 'A', 'C', 'G' }, { 'A', 'A', 'T', 'G', 'C' }, { 'C', 'C', 'G', 'T', 'A' }, { 'G', 'G', 'C', 'A', 'T' } };
            int first = 0, second = 0;
            for (int i = 1; i <= 4; i++)
            {
                if (Box[i, 0].ToString() == key)
                {
                    first = i;
                }
            }
            for (int i = 1; i <= 4; i++)
            {
                if (Box[0, i].ToString() == Number)
                {
                    second = i;
                }
            }
            return (Box[first, second].ToString());
        }


        public static string s_box_X_nor(string key, string keyAlpha, string text)
        {
            char[,] Box = new char[4, 4] { { 'A', 'C', 'T', 'G' }, { 'T', 'A', 'G', 'C' }, { 'G', 'T', 'C', 'A' }, { 'C', 'G', 'A', 'T' } };
            char[,] XNOR = new char[5, 5] { { '0', 'T', 'A', 'C', 'G' }, { 'T', 'T', 'A', 'C', 'G' }, { 'A', 'A', 'T', 'G', 'C' }, { 'C', 'C', 'G', 'T', 'A' }, { 'G', 'G', 'C', 'A', 'T' } };

            string encryptedText = null;
            string finalEncryption = null;
            string firstIndex = Convert.ToString(Convert.ToInt32(key, 2), 10);
            int keyBit = int.Parse(firstIndex);
            for (int i = 0; i < text.Length; i += 2)
            {
                string breaked = text.Substring(i, 2);
                int secondindex = int.Parse(Convert.ToString(Convert.ToInt32(breaked, 2), 10));
                encryptedText += Box[secondindex, keyBit].ToString();
            }
            int first = 0, second = 0;
            for (int j = 0; j <= 4; j++)
            {
                if (XNOR[j, 0].ToString() == keyAlpha)
                {
                    first = j;
                }
            }
            string ch = null;
            for (int k = 0; k < encryptedText.Length; k++)
            {
                ch = encryptedText.Substring(k, 1);
                for (int i = 1; i <= 4; i++)
                {
                    if (XNOR[0, i].ToString() == ch)
                    {
                        second = i;
                    }
                }
                finalEncryption += XNOR[first, second];
            }

            return finalEncryption;
        }

        public static string decryptS_box_XNOR(string text,string keyAlpha,string key)
        {
            char[,] Box = new char[4, 4] { { 'A', 'C', 'T', 'G' }, { 'T', 'A', 'G', 'C' }, { 'G', 'T', 'C', 'A' }, { 'C', 'G', 'A', 'T' } };
            char[,] XNOR = new char[5, 5] { { '0', 'T', 'A', 'C', 'G' }, { 'T', 'T', 'A', 'C', 'G' }, { 'A', 'A', 'T', 'G', 'C' }, { 'C', 'C', 'G', 'T', 'A' }, { 'G', 'G', 'C', 'A', 'T' } };

            int first = 0, second = 0;
            char XNORalpha;
            string x;
            string finalDecrypted = null;
            for (int j = 0; j <= 4; j++)
            {
                if (XNOR[0,j].ToString() == keyAlpha)
                {
                    first = j;
                }
            }
            string ch = null;
            int index = 0;
            for (int k = 0; k < text.Length; k++)
            {
                ch = text.Substring(k, 1);
                for (int i = 1; i <= 4; i++)
                {
                    if (XNOR[i,first].ToString() == ch)
                    {
                        second = i;
                    }
                }
                XNORalpha = XNOR[second,0];

                string firstIndex = Convert.ToString(Convert.ToInt32(key, 2), 10);
                int keyBit = int.Parse(firstIndex);

                for (int q=0;q<4;q++)
                {
                    if(Box[q,keyBit]==XNORalpha)
                    {
                        index = q;
                    }
                }

                x= Convert.ToString(Convert.ToInt32(index.ToString(), 10), 2);
                if(x=="0")
                {
                    x += "0";
                }
                else if(x=="1")
                {
                    x = "0" + x;
                }
                else
                {

                }
                finalDecrypted += x;
            }
            return finalDecrypted;
        }

        public static string keyGeneration()
        {
            string b1, b2;
            b1 = randomBinary();
            b2 = randomBinary();
            b1 = binaryComplement(b1);
            b2 = binaryComplement(b2);
            string key = b1.Substring(2, 2) + b2.Substring(0, 2);
            string Number = b1.Substring(0, 2) + b2.Substring(2, 2);
            encryptedKey = key.Substring(0, 2);
            key = S_box(key);
            Number = S_box(Number);
            return (XNOR(key, Number).ToString());
            
        }

        public static string encryptRailFence(string plaintext, string key)
        {
            string ciphertext = null;
            int j = 0, k = 0;
            char[] ptca = plaintext.ToCharArray();
            char[,] railarray = new char[2, 1000];
            for (int i = 0; i < ptca.Length; ++i)
            {
                if (i % 2 == 0)
                {
                    railarray[0, j] = ptca[i];
                    ++j;
                }
                else
                {
                    railarray[1, k] = ptca[i];
                    ++k;
                }
            }
            railarray[0, j] = '\0';
            railarray[1, k] = '\0';
            for (int x = 0; x < 2; ++x)
            {
                for (int y = 0; railarray[x, y] != '\0'; ++y) ciphertext += railarray[x, y];
            }
            return ciphertext;
        }

        public static string Decrypt(string ciphertext, string key)
        {
            string plaintext = null;
            int j = 0, k = 0, mid;
            char[] ctca = ciphertext.ToCharArray();
            char[,] railarray = new char[2, 1000];
            if (ctca.Length % 2 == 0) mid = ((ctca.Length) / 2) - 1;
            else mid = (ctca.Length) / 2;
            for (int i = 0; i < ctca.Length; ++i)
            {
                if (i <= mid)
                {
                    railarray[0, j] = ctca[i];
                    ++j;
                }
                else
                {
                    railarray[1, k] = ctca[i];
                    ++k;
                }
            }
            railarray[0, j] = '\0';
            railarray[1, k] = '\0';
            for (int x = 0; x <= mid; ++x)
            {
                if (railarray[0, x] != '\0') plaintext += railarray[0, x];
                if (railarray[1, x] != '\0') plaintext += railarray[1, x];
            }
            return plaintext;
        }

        private void ListFiles(DriveService service, ref string pageToken)
        {
            FilesResource.ListRequest listRequest = service.Files.List();
            listRequest.PageSize = 1;
            listRequest.Fields = "nextPageToken, files(name)";
            listRequest.PageToken = pageToken;
            listRequest.Q = "mimeType='txt/*'";

            var request = listRequest.Execute();

            if (request.Files != null && request.Files.Count > 0)
            {
                foreach (var file in request.Files)
                {
                }

                pageToken = request.NextPageToken;

                if (request.NextPageToken != null)
                {
                    Console.ReadLine();
                }
            }
            else
            {
            }
        }

        private void UploadFile(string path, DriveService service, string folderUpload)
        {
            var fileMetadata = new Google.Apis.Drive.v3.Data.File();
            fileMetadata.Name = Path.GetFileName(path);
            fileMetadata.MimeType = "txt/*";

            FilesResource.CreateMediaUpload request;
            using (var stream = new System.IO.FileStream(path, System.IO.FileMode.Open))
            {
                request = service.Files.Create(fileMetadata, stream, "txt/*");
                request.Fields = "id";
                request.Upload();
            }

            var file = request.ResponseBody;

        }

        private void UploadFileCloudB(string path, DriveService service, string folderUpload)
        {
            var fileMetadata = new Google.Apis.Drive.v3.Data.File();
            fileMetadata.Name = Path.GetFileName(path);
            fileMetadata.MimeType = "txt/*";

            FilesResource.CreateMediaUpload request;
            using (var stream = new System.IO.FileStream(path, System.IO.FileMode.Open))
            {
                request = service.Files.Create(fileMetadata, stream, "txt/*");
                request.Fields = "id";
                request.Upload();
            }

            var file = request.ResponseBody;

        }

        private UserCredential GetCredentials()
        {
            UserCredential credential;

            using (var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);

                credPath = Path.Combine(credPath, "client_secret.json");

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
            }

            return credential;
        }

        private UserCredential GetCredentialsCloudB()
        {
            UserCredential credential;

            using (var stream = new FileStream("cloudB.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);

                credPath = Path.Combine(credPath, "cloudB.json");

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
            }

            return credential;
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {

        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if(radioButton1.Checked)
            {
                try
                {
                    string ASCII=convertToASCII(plainText);
                    removeFileContent(loadFileName);
                    string binary = ASCIIToBinary(plainText);
                    write(binary,loadFileName);
                    string key = keyGeneration();
                    removeFileContent(loadFileName);
                    ABC = encryptRailFence(binary, key);
                    string a = s_box_X_nor(encryptedKey, keyGeneration(), ABC);
                    write(a + "$$$" + key + "$$$" + encryptedKey + "$$$",loadFileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Stream browse = null;
            OpenFileDialog readfile = new OpenFileDialog();
            readfile.InitialDirectory = "c:\\";
            readfile.Filter = "txt file (*.txt)|*.txt";
            if (readfile.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string fileName = readfile.FileName;
                    if ((browse = readfile.OpenFile()) != null)
                    {
                        using (browse)
                        {
                            plainText = System.IO.File.ReadAllText(readfile.FileName);
                        }
                    }
                    textBox1.Text = fileName;
                    loadFileName = readfile.SafeFileName;
                    int index=loadFileName.IndexOf(".");
                    StringBuilder sr = new StringBuilder(loadFileName);
                    sr.Remove(index, 4);
                    loadFileName = sr.ToString();
                }
                catch (Exception ex)
                {

                    MessageBox.Show(ex.Message);
                }
            }

        }

        private void upload_btn_Click(object sender, EventArgs e)
        {
            UserCredential credential;
            credential = GetCredentials();
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });
            string folderid;
            var fileMetadatas = new Google.Apis.Drive.v3.Data.File()
            {
                Name = "Encryption",
                MimeType = "application/vnd.google-apps.folder"
            };
            var requests = service.Files.Create(fileMetadatas);
            requests.Fields = "id";
            var files = requests.Execute();
            folderid = files.Id;
            if(radioButton1.Checked)
            {
                string filename =loadFileName+ "Encrypted.txt";
                Thread thread = new Thread(() =>
                {
                    UploadFile(filename, service, folderid);
                });
                thread.IsBackground = true;
                thread.Start();

            }
            else if(radioButton2.Checked)
            {
                
                    string filename = loadFileName+"Encrypted1.txt";
                    Thread thread = new Thread(() =>
                    {
                        UploadFile(filename, service, folderid);
                    });
                    thread.IsBackground = true;
                    thread.Start();
                    UserCredential credentialCloudB;
                    credentialCloudB = GetCredentialsCloudB();
                    var serviceB = new DriveService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = credentialCloudB,
                        ApplicationName = ApplicationName,
                    });
                    string folderidB;
                    var fileMetadatasB = new Google.Apis.Drive.v3.Data.File()
                    {
                        Name = "Encryption",
                        MimeType = "application/vnd.google-apps.folder"
                    };
                    var requestsB = service.Files.Create(fileMetadatas);
                    requests.Fields = "id";
                    var filesB = requests.Execute();
                    folderidB = files.Id;
                    string filenameB = loadFileName + "Encrypted2.txt";
                    Thread threadB = new Thread(() =>
                    {
                        UploadFile(filenameB, serviceB, folderidB);
                    });
                    threadB.IsBackground = true;
                    threadB.Start();
            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if(radioButton2.Checked)
            {
                try
                {
                   string ASCII= convertToASCII(plainText);
                   clearFileContent(loadFileName);
                   string binary = ASCIIToBinary(plainText);
                   writeOnFile(binary,loadFileName);
                   string key = keyGeneration();
                   clearFileContent(loadFileName);
                   ABC = encryptRailFence(binary, key);
                   
                  string a= s_box_X_nor(encryptedKey, keyGeneration(), ABC);
                  writeOnFile(a + "$$$" + key + "$$$" + encryptedKey + "$$$",loadFileName);
                     
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

        }

        private void decrypt_btn_Click(object sender, EventArgs e)
        {
            string path = @"";
            string Filename = textBox2.Text;
            var newpath = path + Filename;
            plainText=System.IO.File.ReadAllText(@newpath);
            try
                {
                    int index = plainText.IndexOf('$');
                    int keyIndex = index + 3;
                    int keyBitIndex = keyIndex + 4;
                    string keyAlpha = plainText.Substring(keyIndex, 1);
                    string keyBit = plainText.Substring(keyBitIndex, 2);
                    StringBuilder sb = new StringBuilder(plainText);
                    sb.Remove(index, 14);
                    string cipherText = sb.ToString();
                   string binary = Decrypt(decryptS_box_XNOR(cipherText, keyAlpha, keyBit), keyAlpha);
                   var data = GetBytesFromBinaryString(binary);
                   var text = Encoding.ASCII.GetString(data);
                   removeFileContentDecoded(loadFileName);
                   writeDecoded(text,loadFileName);

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
        }

    }
}
