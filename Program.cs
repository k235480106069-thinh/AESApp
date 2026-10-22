
using System;
using System.IO;
using System.Security.Cryptography;

Console.WriteLine("=== AES MA HOA FILE ===");

string keyPath = Path.Combine(
    AppContext.BaseDirectory, "aes_key.txt");

if (!File.Exists(keyPath))
{
    Console.WriteLine("Khong tim thay aes_key.txt. Hay sinh khoa truoc.");
    return;
}

byte[] key = Convert.FromBase64String(
    File.ReadAllText(keyPath).Trim());

if (key.Length != 32)
{
    Console.WriteLine("Khoa khong hop le.");
    return;
}

string inputPath = @"C:\AES_Test\large_test.bin";
string encryptedPath = @"C:\AES_Test\large_test.aes";
string decryptedPath = @"C:\AES_Test\large_test_decrypted.bin";

if (!File.Exists(inputPath))
{
    Console.WriteLine($"Khong tim thay file: {inputPath}");
    return;
}

// Ma hoa file: IV duoc ghi 16 byte dau tien
DateTime encryptStart = DateTime.Now;

byte[] iv = RandomNumberGenerator.GetBytes(16);

using (FileStream input = new FileStream(
    inputPath, FileMode.Open, FileAccess.Read))
using (FileStream output = new FileStream(
    encryptedPath, FileMode.Create, FileAccess.Write))
{
    // Luu IV vao dau file ma hoa
    output.Write(iv, 0, iv.Length);

    using Aes aes = Aes.Create();
    aes.Key = key;
    aes.IV = iv;
    aes.Mode = CipherMode.CBC;
    aes.Padding = PaddingMode.PKCS7;

    using CryptoStream crypto = new CryptoStream(
        output, aes.CreateEncryptor(), CryptoStreamMode.Write);

    input.CopyTo(crypto);
}

TimeSpan encryptTime = DateTime.Now - encryptStart;

// Giai ma file: doc IV tu 16 byte dau
DateTime decryptStart = DateTime.Now;

using (FileStream input = new FileStream(
    encryptedPath, FileMode.Open, FileAccess.Read))
using (FileStream output = new FileStream(
    decryptedPath, FileMode.Create, FileAccess.Write))
{
    byte[] readIv = new byte[16];

    int read = input.Read(readIv, 0, readIv.Length);

    if (read != 16)
    {
        Console.WriteLine("File ma hoa khong hop le.");
        return;
    }

    using Aes aes = Aes.Create();
    aes.Key = key;
    aes.IV = readIv;
    aes.Mode = CipherMode.CBC;
    aes.Padding = PaddingMode.PKCS7;

    using CryptoStream crypto = new CryptoStream(
        input, aes.CreateDecryptor(), CryptoStreamMode.Read);

    crypto.CopyTo(output);
}

TimeSpan decryptTime = DateTime.Now - decryptStart;

// So sanh theo byte, doc tung khoi
bool identical = CompareFiles(inputPath, decryptedPath);

Console.WriteLine("\n--- KET QUA ---");
Console.WriteLine($"File goc: {inputPath}");
Console.WriteLine($"File ma hoa: {encryptedPath}");
Console.WriteLine($"File giai ma: {decryptedPath}");
Console.WriteLine($"Thoi gian ma hoa: {encryptTime.TotalSeconds:F4} giay");
Console.WriteLine($"Thoi gian giai ma: {decryptTime.TotalSeconds:F4} giay");
Console.WriteLine($"Kich thuoc file goc: {new FileInfo(inputPath).Length} byte");
Console.WriteLine($"Kich thuoc file ma hoa: {new FileInfo(encryptedPath).Length} byte");
Console.WriteLine($"Kich thuoc file giai ma: {new FileInfo(decryptedPath).Length} byte");
Console.WriteLine($"File goc va file giai ma giong nhau: {identical}");

static bool CompareFiles(string path1, string path2)
{
    const int bufferSize = 1024 * 1024; // 1 MB

    using FileStream file1 = File.OpenRead(path1);
    using FileStream file2 = File.OpenRead(path2);

    if (file1.Length != file2.Length)
        return false;

    byte[] buffer1 = new byte[bufferSize];
    byte[] buffer2 = new byte[bufferSize];

    while (true)
    {
        int count1 = file1.Read(buffer1, 0, buffer1.Length);
        int count2 = file2.Read(buffer2, 0, buffer2.Length);

        if (count1 != count2)
            return false;

        if (count1 == 0)
            return true;

        for (int i = 0; i < count1; i++)
        {
            if (buffer1[i] != buffer2[i])
                return false;
        }
    }
}
