﻿using System.Data.SqlServerCe;
// ReSharper disable InconsistentNaming

namespace PeanutButter.TempDb.SqlCe
{
    public class TempDBSqlCe : TempDB<SqlCeConnection>
    {
        public TempDBSqlCe(params string[] creationScripts)
            : base(creationScripts)
        {
        }
        protected override void CreateDatabase()
        {
            using (var engine = new SqlCeEngine(ConnectionString))
            {
                engine.CreateDatabase();
            }
        }
    }
}
