﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Threading;
using PeanutButter.Utils;
// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable VirtualMemberCallInConstructor
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMemberInSuper.Global

namespace PeanutButter.TempDb
{
    // ReSharper disable once InconsistentNaming
    public interface ITempDB : IDisposable
    {
        string DatabaseFile { get; }
        string ConnectionString { get; }
        DbConnection CreateConnection();
    }

    public abstract class TempDB<TDatabaseConnection> : ITempDB where TDatabaseConnection: DbConnection
    {
        public string DatabaseFile { get; private set; }
        public string ConnectionString { get { return GenerateConnectionString(); } }
        // ReSharper disable once StaticMemberInGenericType
        private static readonly Semaphore _lock = new Semaphore(1, 1);
        private List<DbConnection> _managedConnections;

        public TempDB(params string[] creationScripts)
        {
            Init(creationScripts);
        }

        public TempDB(Action<object> beforeInit, params string[] creationScripts)
        {
            beforeInit(this);
            Init(creationScripts);
        }

        protected virtual void Init(string[] creationScripts)
        {
            try
            {
                AttemptToCreateDatabaseWith(TempDbHints.PreferredBasePath);
            }
            catch (Exception ex)
            {
                if (TempDbHints.UsingOverrideBasePath)
                {
                    AttemptToCreateDatabaseWith(Path.GetTempPath());
                    System.Diagnostics.Trace.WriteLine("An error was encountered whilst attempting to use the configured TempDbHints.PreferredBasePath: " + ex.Message);
                    System.Diagnostics.Trace.WriteLine(" -> falling back on using %TEMP%");
                    TempDbHints.PreferredBasePath = TempDbHints.DefaultBasePath;
                }
            }
            _managedConnections = new List<DbConnection>();
            RunScripts(creationScripts);
        }

        private void AttemptToCreateDatabaseWith(string basePath)
        {
            using (new AutoLocker(_lock))
            {
                do
                {
                    DatabaseFile = Path.Combine(basePath, Guid.NewGuid().ToString() + ".db");
                } while (File.Exists(DatabaseFile));
                CreateDatabase();
            }
        }

        protected virtual string GenerateConnectionString()
        {
            return $"DataSource=\"{DatabaseFile}\"";
        }

        protected abstract void CreateDatabase();

        public virtual DbConnection CreateConnection()
        {
            var connection = CreateOpenDatabaseConnection();
            _managedConnections.Add(connection);
            return connection;
        }

        private DbConnection CreateOpenDatabaseConnection()
        {
            var connection = Activator.CreateInstance(typeof (TDatabaseConnection), ConnectionString) as DbConnection;
            if (connection != null)
            {
                connection.Open();
                return connection;
            }
            else throw new InvalidOperationException($"Unable to instantate connection of type {typeof(TDatabaseConnection)} as a DbConnection");
        }

        private void RunScripts(IEnumerable<string> scripts)
        {
            if (scripts == null || !scripts.Any())
                return;
            using (var disposer = new AutoDisposer())
            {
                var conn = disposer.Add(CreateOpenDatabaseConnection());
                var cmd = disposer.Add(conn.CreateCommand());
                Action<string> exec = s => {
                                               cmd.CommandText = s;
                                               cmd.ExecuteNonQuery();
                };
                foreach (var script in scripts)
                    exec(script);
            }
        }

        public virtual void Dispose()
        {
            lock (this)
            {
                DisposeOfManagedConnections();
                DeleteTemporaryDatabaseFile();
            }
        }

        protected virtual void DeleteTemporaryDatabaseFile()
        {
            try
            {
                File.Delete(DatabaseFile);
                DatabaseFile = null;
            }
            catch
            {
                System.Diagnostics.Trace.WriteLine(
                    $"WARNING: Unable to delete temporary database at: {DatabaseFile}; probably still locked. Artifact will remain on disk.");
            }
        }

        private void Trace(string message)
        {
            System.Diagnostics.Trace.WriteLine(message);
        }

        private void DisposeOfManagedConnections()
        {
            Action<string, Action> TryDo = (heading, toRun) =>
            {
                try { toRun(); }
                catch (Exception ex)
                {
                    Trace("Error whilst: " + heading);
                    Trace(ex.Message);
                }
            };
            foreach (var conn in _managedConnections)
            {
                var localConnection = conn;
                TryDo("disposing of managed connections", localConnection.Dispose);
            }
        }
    }
}