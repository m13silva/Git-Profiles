using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;

namespace Git_profiles.Models
{
    public class DatabaseService
    {
        private readonly string _databasePath;

        public DatabaseService()
        {
            _databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GitProfiles.db");
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            var connectionString = new SqliteConnectionStringBuilder
            {
                DataSource = _databasePath,
                Mode = SqliteOpenMode.ReadWriteCreate
            }.ToString();

            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS GitProfiles (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Email TEXT NOT NULL,
                    GpgKeyId TEXT,
                    Color TEXT,
                    IsActive INTEGER DEFAULT 0,
                    UseGpg INTEGER DEFAULT 0
                )";
            command.ExecuteNonQuery();
        }

        public List<GitProfileModel> GetAllProfiles()
        {
            var profiles = new List<GitProfileModel>();
            var connectionString = new SqliteConnectionStringBuilder
            {
                DataSource = _databasePath,
                Mode = SqliteOpenMode.ReadWrite
            }.ToString();

            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM GitProfiles";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                profiles.Add(new GitProfileModel
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Email = reader.GetString(2),
                    GpgKeyId = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    Color = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                    IsActive = reader.GetInt32(5) == 1,
                    UseGpg = reader.GetInt32(6) == 1
                });
            }

            return profiles;
        }

        public GitProfileModel SaveProfile(GitProfileModel profile)
        {
            var connectionString = new SqliteConnectionStringBuilder
            {
                DataSource = _databasePath,
                Mode = SqliteOpenMode.ReadWrite
            }.ToString();

            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            using var transaction = connection.BeginTransaction();
            try
            {
                var command = connection.CreateCommand();
                if (profile.Id == 0)
                {
                    command.CommandText = @"
                INSERT INTO GitProfiles (Name, Email, GpgKeyId, Color, IsActive, UseGpg)
                VALUES (@name, @email, @gpgKeyId, @color, @isActive, @useGpg);
                SELECT last_insert_rowid();";
                }
                else
                {
                    command.CommandText = @"
                UPDATE GitProfiles 
                SET Name = @name, Email = @email, GpgKeyId = @gpgKeyId, 
                    Color = @color, IsActive = @isActive, UseGpg = @useGpg
                WHERE Id = @id;
                SELECT @id;";
                    command.Parameters.AddWithValue("@id", profile.Id);
                }

                command.Parameters.AddWithValue("@name", profile.Name);
                command.Parameters.AddWithValue("@email", profile.Email);
                command.Parameters.AddWithValue("@gpgKeyId", (object)profile.GpgKeyId ?? DBNull.Value);
                command.Parameters.AddWithValue("@color", (object)profile.Color ?? DBNull.Value);
                command.Parameters.AddWithValue("@isActive", profile.IsActive ? 1 : 0);
                command.Parameters.AddWithValue("@useGpg", profile.UseGpg ? 1 : 0);

                // Ejecutar el comando y obtener el ID
                profile.Id = Convert.ToInt32(command.ExecuteScalar());

                transaction.Commit();
                return profile;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void DeleteProfile(int profileId)
        {
            var connectionString = new SqliteConnectionStringBuilder
            {
                DataSource = _databasePath,
                Mode = SqliteOpenMode.ReadWrite
            }.ToString();

            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM GitProfiles WHERE Id = @id";
            command.Parameters.AddWithValue("@id", profileId);
            command.ExecuteNonQuery();
        }
    }
}