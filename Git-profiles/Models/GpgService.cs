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
                        Arguments = "/c gpg --list-secret-keys --keyid-format=long",
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
                    var keyRegex = new Regex(@"sec\s+(?:[\w-]+\/)?(\w+)\s+(\d{4}-\d{2}-\d{2})\s+\[(?:SC|S|E)\]\s*\n(?:.*\n)*?\s*uid\s+\[ultimate\]\s+([^\n]+)", RegexOptions.Multiline);
                    var matches = keyRegex.Matches(output);

                    foreach (Match match in matches)
                    {
                        keys.Add(new GpgKey
                        {
                            KeyId = match.Groups[1].Value,
                            CreationDate = DateTime.Parse(match.Groups[2].Value),
                            UserInfo = match.Groups[3].Value
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