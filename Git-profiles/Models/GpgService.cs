using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Git_profiles.Models
{
    public class GpgService
    {
        public static async Task<List<GpgKey>> GetGpgKeys()
        {
            var keys = new List<GpgKey>();

            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = "/c gpg --list-secret-keys --with-colons",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();

                if (process.ExitCode == 0)
                {
                    var lines = output.Split('\n');
                    string currentKeyId = "";
                    DateTime currentCreationDate = DateTime.MinValue;
                    string currentUserInfo = "";

                    foreach (var line in lines)
                    {
                        var fields = line.Split(':');
                        if (fields.Length < 10) continue;

                        if (fields[0] == "sec")
                        {
                            // Si ya teníamos una llave anterior, la agregamos a la lista
                            if (!string.IsNullOrEmpty(currentKeyId) && !string.IsNullOrEmpty(currentUserInfo))
                            {
                                keys.Add(new GpgKey
                                {
                                    KeyId = currentKeyId,
                                    CreationDate = currentCreationDate,
                                    UserInfo = currentUserInfo
                                });
                            }

                            currentKeyId = fields[4];
                            currentCreationDate = DateTimeOffset.FromUnixTimeSeconds(long.Parse(fields[5])).DateTime;
                            currentUserInfo = "";
                        }
                        else if (fields[0] == "uid" && string.IsNullOrEmpty(currentUserInfo))
                        {
                            currentUserInfo = fields[9];
                        }
                    }

                    // Agregar la última llave
                    if (!string.IsNullOrEmpty(currentKeyId) && !string.IsNullOrEmpty(currentUserInfo))
                    {
                        keys.Add(new GpgKey
                        {
                            KeyId = currentKeyId,
                            CreationDate = currentCreationDate,
                            UserInfo = currentUserInfo
                        });
                    }
                }
                else
                {
                    Console.WriteLine($"GPG command failed with error: {error}");
                }
            }
            catch (Exception ex)
            {
                // Log or handle the error appropriately
                Console.WriteLine($"Error getting GPG keys: {ex.Message}");
            }

            return keys;
        }
    }

    public class GpgKey
    {
        public string KeyId { get; set; } = string.Empty;
        public DateTime CreationDate { get; set; }
        public string UserInfo { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"{KeyId} - {UserInfo}";
        }
    }
}