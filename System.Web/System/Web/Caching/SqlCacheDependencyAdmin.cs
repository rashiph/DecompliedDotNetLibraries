namespace System.Web.Caching
{
    using System;
    using System.Collections;
    using System.Data;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.Security.Permissions;
    using System.Web;

    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.High)]
    public static class SqlCacheDependencyAdmin
    {
        internal const string DROP_MEMBERS = "CREATE TABLE #aspnet_RoleMembers \n( \n    Group_name      sysname, \n    Group_id        smallint, \n    Users_in_group  sysname, \n    User_id         smallint \n) \nINSERT INTO #aspnet_RoleMembers \nEXEC sp_helpuser 'aspnet_ChangeNotification_ReceiveNotificationsOnlyAccess' \n \nDECLARE @user_id smallint \nDECLARE @cmd nvarchar(500) \nDECLARE c1 CURSOR FORWARD_ONLY FOR  \n    SELECT User_id FROM #aspnet_RoleMembers \n  \nOPEN c1 \n  \nFETCH c1 INTO @user_id \nWHILE (@@fetch_status = 0)  \nBEGIN \n    SET @cmd = 'EXEC sp_droprolemember ''aspnet_ChangeNotification_ReceiveNotificationsOnlyAccess'',''' + USER_NAME(@user_id) + '''' \n    EXEC (@cmd) \n    FETCH c1 INTO @user_id \nEND \n \nclose c1 \ndeallocate c1 \n";
        private const int SETUP_DISABLE = 2;
        private const int SETUP_HTTPREQUEST = 4;
        private const int SETUP_TABLE = 1;
        private const int SETUP_TABLES = 8;
        internal const string SQL_CREATE_ENABLE_DATABASE_SP = "/* Create notification table */ \nIF NOT EXISTS (SELECT name FROM sysobjects WITH (NOLOCK) WHERE name = '{0}' AND type = 'U') \n   IF NOT EXISTS (SELECT name FROM sysobjects WITH (TABLOCKX) WHERE name = '{0}' AND type = 'U') \n      CREATE TABLE dbo.{0} (\n      tableName             NVARCHAR(450) NOT NULL PRIMARY KEY,\n      notificationCreated   DATETIME NOT NULL DEFAULT(GETDATE()),\n      changeId              INT NOT NULL DEFAULT(0)\n      )\n\n/* Create polling SP */\nIF NOT EXISTS (SELECT name FROM sysobjects WITH (NOLOCK) WHERE name = '{1}' AND type = 'P') \n   IF NOT EXISTS (SELECT name FROM sysobjects WITH (TABLOCKX) WHERE name = '{1}' AND type = 'P') \n   EXEC('CREATE PROCEDURE dbo.{1} AS\n         SELECT tableName, changeId FROM dbo.{0}\n         RETURN 0')\n\n/* Create SP for registering a table. */ \nIF NOT EXISTS (SELECT name FROM sysobjects WITH (NOLOCK) WHERE name = '{2}' AND type = 'P') \n   IF NOT EXISTS (SELECT name FROM sysobjects WITH (TABLOCKX) WHERE name = '{2}' AND type = 'P') \n   EXEC('CREATE PROCEDURE dbo.{2} \n             @tableName NVARCHAR(450) \n         AS\n         BEGIN\n\n         DECLARE @triggerName AS NVARCHAR(3000) \n         DECLARE @fullTriggerName AS NVARCHAR(3000)\n         DECLARE @canonTableName NVARCHAR(3000) \n         DECLARE @quotedTableName NVARCHAR(3000) \n\n         /* Create the trigger name */ \n         SET @triggerName = REPLACE(@tableName, ''['', ''__o__'') \n         SET @triggerName = REPLACE(@triggerName, '']'', ''__c__'') \n         SET @triggerName = @triggerName + ''{3}'' \n         SET @fullTriggerName = ''dbo.['' + @triggerName + '']'' \n\n         /* Create the cannonicalized table name for trigger creation */ \n         /* Do not touch it if the name contains other delimiters */ \n         IF (CHARINDEX(''.'', @tableName) <> 0 OR \n             CHARINDEX(''['', @tableName) <> 0 OR \n             CHARINDEX('']'', @tableName) <> 0) \n             SET @canonTableName = @tableName \n         ELSE \n             SET @canonTableName = ''['' + @tableName + '']'' \n\n         /* First make sure the table exists */ \n         IF (SELECT OBJECT_ID(@tableName, ''U'')) IS NULL \n         BEGIN \n             RAISERROR (''00000001'', 16, 1) \n             RETURN \n         END \n\n         BEGIN TRAN\n         /* Insert the value into the notification table */ \n         IF NOT EXISTS (SELECT tableName FROM dbo.{0} WITH (NOLOCK) WHERE tableName = @tableName) \n             IF NOT EXISTS (SELECT tableName FROM dbo.{0} WITH (TABLOCKX) WHERE tableName = @tableName) \n                 INSERT  dbo.{0} \n                 VALUES (@tableName, GETDATE(), 0)\n\n         /* Create the trigger */ \n         SET @quotedTableName = QUOTENAME(@tableName, '''''''') \n         IF NOT EXISTS (SELECT name FROM sysobjects WITH (NOLOCK) WHERE name = @triggerName AND type = ''TR'') \n             IF NOT EXISTS (SELECT name FROM sysobjects WITH (TABLOCKX) WHERE name = @triggerName AND type = ''TR'') \n                 EXEC(''CREATE TRIGGER '' + @fullTriggerName + '' ON '' + @canonTableName +''\n                       FOR INSERT, UPDATE, DELETE AS BEGIN\n                       SET NOCOUNT ON\n                       EXEC dbo.{6} N'' + @quotedTableName + ''\n                       END\n                       '')\n         COMMIT TRAN\n         END\n   ')\n\n/* Create SP for updating the change Id of a table. */ \nIF NOT EXISTS (SELECT name FROM sysobjects WITH (NOLOCK) WHERE name = '{6}' AND type = 'P') \n   IF NOT EXISTS (SELECT name FROM sysobjects WITH (TABLOCKX) WHERE name = '{6}' AND type = 'P') \n   EXEC('CREATE PROCEDURE dbo.{6} \n             @tableName NVARCHAR(450) \n         AS\n\n         BEGIN \n             UPDATE dbo.{0} WITH (ROWLOCK) SET changeId = changeId + 1 \n             WHERE tableName = @tableName\n         END\n   ')\n\n/* Create SP for unregistering a table. */ \nIF NOT EXISTS (SELECT name FROM sysobjects WITH (NOLOCK) WHERE name = '{4}' AND type = 'P') \n   IF NOT EXISTS (SELECT name FROM sysobjects WITH (TABLOCKX) WHERE name = '{4}' AND type = 'P') \n   EXEC('CREATE PROCEDURE dbo.{4} \n             @tableName NVARCHAR(450) \n         AS\n         BEGIN\n\n         BEGIN TRAN\n         DECLARE @triggerName AS NVARCHAR(3000) \n         DECLARE @fullTriggerName AS NVARCHAR(3000)\n         SET @triggerName = REPLACE(@tableName, ''['', ''__o__'') \n         SET @triggerName = REPLACE(@triggerName, '']'', ''__c__'') \n         SET @triggerName = @triggerName + ''{3}'' \n         SET @fullTriggerName = ''dbo.['' + @triggerName + '']'' \n\n         /* Remove the table-row from the notification table */ \n         IF EXISTS (SELECT name FROM sysobjects WITH (NOLOCK) WHERE name = ''{0}'' AND type = ''U'') \n             IF EXISTS (SELECT name FROM sysobjects WITH (TABLOCKX) WHERE name = ''{0}'' AND type = ''U'') \n             DELETE FROM dbo.{0} WHERE tableName = @tableName \n\n         /* Remove the trigger */ \n         IF EXISTS (SELECT name FROM sysobjects WITH (NOLOCK) WHERE name = @triggerName AND type = ''TR'') \n             IF EXISTS (SELECT name FROM sysobjects WITH (TABLOCKX) WHERE name = @triggerName AND type = ''TR'') \n             EXEC(''DROP TRIGGER '' + @fullTriggerName) \n\n         COMMIT TRAN\n         END\n   ')\n\n/* Create SP for querying all registered table */ \nIF NOT EXISTS (SELECT name FROM sysobjects WITH (NOLOCK) WHERE name = '{5}' AND type = 'P') \n   IF NOT EXISTS (SELECT name FROM sysobjects WITH (TABLOCKX) WHERE name = '{5}' AND type = 'P') \n   EXEC('CREATE PROCEDURE dbo.{5} \n         AS\n         SELECT tableName FROM dbo.{0}   ')\n\n/* Create roles and grant them access to SP  */ \nIF NOT EXISTS (SELECT name FROM sysusers WHERE issqlrole = 1 AND name = N'aspnet_ChangeNotification_ReceiveNotificationsOnlyAccess') \n    EXEC sp_addrole N'aspnet_ChangeNotification_ReceiveNotificationsOnlyAccess' \n\nGRANT EXECUTE ON dbo.{1} to aspnet_ChangeNotification_ReceiveNotificationsOnlyAccess\n\n";
        internal const string SQL_DISABLE_DATABASE = "/* Remove notification table */ \nIF EXISTS (SELECT name FROM sysobjects WITH (NOLOCK) WHERE name = '{0}' AND type = 'U') \n    IF EXISTS (SELECT name FROM sysobjects WITH (TABLOCKX) WHERE name = '{0}' AND type = 'U') \n    BEGIN\n      /* First, unregister all registered tables */ \n      DECLARE tables_cursor CURSOR FOR \n      SELECT tableName FROM dbo.{0} \n      DECLARE @tableName AS NVARCHAR(450) \n\n      OPEN tables_cursor \n\n      /* Perform the first fetch. */ \n      FETCH NEXT FROM tables_cursor INTO @tableName \n\n      /* Check @@FETCH_STATUS to see if there are any more rows to fetch. */ \n      WHILE @@FETCH_STATUS = 0 \n      BEGIN \n          EXEC {3} @tableName \n\n          /* This is executed as long as the previous fetch succeeds. */ \n          FETCH NEXT FROM tables_cursor INTO @tableName \n      END \n      CLOSE tables_cursor \n      DEALLOCATE tables_cursor \n\n      /* Drop the table */\n      DROP TABLE dbo.{0} \n    END\n\n/* Remove polling SP */ \nIF EXISTS (SELECT name FROM sysobjects WITH (NOLOCK) WHERE name = '{1}' AND type = 'P') \n    IF EXISTS (SELECT name FROM sysobjects WITH (TABLOCKX) WHERE name = '{1}' AND type = 'P') \n      DROP PROCEDURE dbo.{1} \n\n/* Remove SP that registers a table */ \nIF EXISTS (SELECT name FROM sysobjects WITH (NOLOCK) WHERE name = '{2}' AND type = 'P') \n    IF EXISTS (SELECT name FROM sysobjects WITH (TABLOCKX) WHERE name = '{2}' AND type = 'P') \n      DROP PROCEDURE dbo.{2} \n\n/* Remove SP that unregisters a table */ \nIF EXISTS (SELECT name FROM sysobjects WITH (NOLOCK) WHERE name = '{3}' AND type = 'P') \n    IF EXISTS (SELECT name FROM sysobjects WITH (TABLOCKX) WHERE name = '{3}' AND type = 'P') \n      DROP PROCEDURE dbo.{3} \n\n/* Remove SP that querys the registered table */ \nIF EXISTS (SELECT name FROM sysobjects WITH (NOLOCK) WHERE name = '{4}' AND type = 'P') \n    IF EXISTS (SELECT name FROM sysobjects WITH (TABLOCKX) WHERE name = '{4}' AND type = 'P') \n      DROP PROCEDURE dbo.{4} \n\n/* Remove SP that updates the change Id of a table. */ \nIF EXISTS (SELECT name FROM sysobjects WITH (NOLOCK) WHERE name = '{5}' AND type = 'P') \n    IF EXISTS (SELECT name FROM sysobjects WITH (TABLOCKX) WHERE name = '{5}' AND type = 'P') \n      DROP PROCEDURE dbo.{5} \n\n/* Drop roles */ \nIF EXISTS ( SELECT name FROM sysusers WHERE issqlrole = 1 AND name = 'aspnet_ChangeNotification_ReceiveNotificationsOnlyAccess') BEGIN\nCREATE TABLE #aspnet_RoleMembers \n( \n    Group_name      sysname, \n    Group_id        smallint, \n    Users_in_group  sysname, \n    User_id         smallint \n) \nINSERT INTO #aspnet_RoleMembers \nEXEC sp_helpuser 'aspnet_ChangeNotification_ReceiveNotificationsOnlyAccess' \n \nDECLARE @user_id smallint \nDECLARE @cmd nvarchar(500) \nDECLARE c1 CURSOR FORWARD_ONLY FOR  \n    SELECT User_id FROM #aspnet_RoleMembers \n  \nOPEN c1 \n  \nFETCH c1 INTO @user_id \nWHILE (@@fetch_status = 0)  \nBEGIN \n    SET @cmd = 'EXEC sp_droprolemember ''aspnet_ChangeNotification_ReceiveNotificationsOnlyAccess'',''' + USER_NAME(@user_id) + '''' \n    EXEC (@cmd) \n    FETCH c1 INTO @user_id \nEND \n \nclose c1 \ndeallocate c1 \n    EXEC sp_droprole 'aspnet_ChangeNotification_ReceiveNotificationsOnlyAccess'\nEND\n";
        internal const string SQL_QUERY_REGISTERED_TABLES_SP = "AspNet_SqlCacheQueryRegisteredTablesStoredProcedure";
        internal const string SQL_QUERY_REGISTERED_TABLES_SP_DBO = "dbo.AspNet_SqlCacheQueryRegisteredTablesStoredProcedure";
        internal const string SQL_REGISTER_TABLE_SP = "AspNet_SqlCacheRegisterTableStoredProcedure";
        internal const string SQL_REGISTER_TABLE_SP_DBO = "dbo.AspNet_SqlCacheRegisterTableStoredProcedure";
        internal const string SQL_TRIGGER_NAME_POSTFIX = "_AspNet_SqlCacheNotification_Trigger";
        internal const string SQL_UNREGISTER_TABLE_SP = "AspNet_SqlCacheUnRegisterTableStoredProcedure";
        internal const string SQL_UNREGISTER_TABLE_SP_DBO = "dbo.AspNet_SqlCacheUnRegisterTableStoredProcedure";
        internal const string SQL_UPDATE_CHANGE_ID_SP = "AspNet_SqlCacheUpdateChangeIdStoredProcedure";

        public static void DisableNotifications(string connectionString)
        {
            SetupNotifications(2, null, connectionString);
        }

        public static void DisableTableForNotifications(string connectionString, string table)
        {
            SetupNotifications(3, table, connectionString);
        }

        public static void DisableTableForNotifications(string connectionString, string[] tables)
        {
            if (tables == null)
            {
                throw new ArgumentNullException("tables");
            }
            foreach (string str in tables)
            {
                SetupNotifications(10, str, connectionString);
            }
        }

        public static void EnableNotifications(string connectionString)
        {
            SetupNotifications(0, null, connectionString);
        }

        public static void EnableTableForNotifications(string connectionString, string table)
        {
            SetupNotifications(1, table, connectionString);
        }

        public static void EnableTableForNotifications(string connectionString, string[] tables)
        {
            if (tables == null)
            {
                throw new ArgumentNullException("tables");
            }
            foreach (string str in tables)
            {
                SetupNotifications(8, str, connectionString);
            }
        }

        private static string[] GetEnabledTables(string connectionString)
        {
            SqlDataReader reader = null;
            SqlConnection connection = null;
            ArrayList list = new ArrayList();
            try
            {
                connection = new SqlConnection(connectionString);
                connection.Open();
                reader = new SqlCommand("dbo.AspNet_SqlCacheQueryRegisteredTablesStoredProcedure", connection) { CommandType = CommandType.StoredProcedure }.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(reader.GetString(0));
                }
            }
            catch (Exception exception)
            {
                SqlException exception2 = exception as SqlException;
                if ((exception2 != null) && (exception2.Number == 0xafc))
                {
                    throw new DatabaseNotEnabledForNotificationException(System.Web.SR.GetString("Database_not_enabled_for_notification", new object[] { connection.Database }));
                }
                throw new HttpException(System.Web.SR.GetString("Cant_get_enabled_tables_sql_cache_dep"), exception);
            }
            finally
            {
                try
                {
                    if (reader != null)
                    {
                        reader.Close();
                    }
                    if (connection != null)
                    {
                        connection.Close();
                    }
                }
                catch
                {
                }
            }
            return (string[]) list.ToArray(Type.GetType("System.String"));
        }

        public static string[] GetTablesEnabledForNotifications(string connectionString)
        {
            return GetEnabledTables(connectionString);
        }

        internal static void SetupNotifications(int flags, string table, string connectionString)
        {
            SqlConnection connection = null;
            SqlCommand command = null;
            bool flag = (flags & 9) != 0;
            bool flag2 = (flags & 2) != 0;
            if (flag)
            {
                bool flag3 = (flags & 8) != 0;
                if (table == null)
                {
                    if (flag3)
                    {
                        throw new ArgumentException(System.Web.SR.GetString("Cache_null_table_in_tables"), "tables");
                    }
                    throw new ArgumentNullException("table");
                }
                if (table.Length == 0)
                {
                    if (flag3)
                    {
                        throw new ArgumentException(System.Web.SR.GetString("Cache_null_table_in_tables"), "tables");
                    }
                    throw new ArgumentException(System.Web.SR.GetString("Cache_null_table"), "table");
                }
            }
            try
            {
                connection = new SqlConnection(connectionString);
                connection.Open();
                command = new SqlCommand(null, connection);
                if (flag)
                {
                    command.CommandText = !flag2 ? "dbo.AspNet_SqlCacheRegisterTableStoredProcedure" : "dbo.AspNet_SqlCacheUnRegisterTableStoredProcedure";
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@tableName", SqlDbType.NVarChar, table.Length));
                    command.Parameters[0].Value = table;
                }
                else if (!flag2)
                {
                    command.CommandText = string.Format(CultureInfo.InvariantCulture, "/* Create notification table */ \nIF NOT EXISTS (SELECT name FROM sysobjects WITH (NOLOCK) WHERE name = '{0}' AND type = 'U') \n   IF NOT EXISTS (SELECT name FROM sysobjects WITH (TABLOCKX) WHERE name = '{0}' AND type = 'U') \n      CREATE TABLE dbo.{0} (\n      tableName             NVARCHAR(450) NOT NULL PRIMARY KEY,\n      notificationCreated   DATETIME NOT NULL DEFAULT(GETDATE()),\n      changeId              INT NOT NULL DEFAULT(0)\n      )\n\n/* Create polling SP */\nIF NOT EXISTS (SELECT name FROM sysobjects WITH (NOLOCK) WHERE name = '{1}' AND type = 'P') \n   IF NOT EXISTS (SELECT name FROM sysobjects WITH (TABLOCKX) WHERE name = '{1}' AND type = 'P') \n   EXEC('CREATE PROCEDURE dbo.{1} AS\n         SELECT tableName, changeId FROM dbo.{0}\n         RETURN 0')\n\n/* Create SP for registering a table. */ \nIF NOT EXISTS (SELECT name FROM sysobjects WITH (NOLOCK) WHERE name = '{2}' AND type = 'P') \n   IF NOT EXISTS (SELECT name FROM sysobjects WITH (TABLOCKX) WHERE name = '{2}' AND type = 'P') \n   EXEC('CREATE PROCEDURE dbo.{2} \n             @tableName NVARCHAR(450) \n         AS\n         BEGIN\n\n         DECLARE @triggerName AS NVARCHAR(3000) \n         DECLARE @fullTriggerName AS NVARCHAR(3000)\n         DECLARE @canonTableName NVARCHAR(3000) \n         DECLARE @quotedTableName NVARCHAR(3000) \n\n         /* Create the trigger name */ \n         SET @triggerName = REPLACE(@tableName, ''['', ''__o__'') \n         SET @triggerName = REPLACE(@triggerName, '']'', ''__c__'') \n         SET @triggerName = @triggerName + ''{3}'' \n         SET @fullTriggerName = ''dbo.['' + @triggerName + '']'' \n\n         /* Create the cannonicalized table name for trigger creation */ \n         /* Do not touch it if the name contains other delimiters */ \n         IF (CHARINDEX(''.'', @tableName) <> 0 OR \n             CHARINDEX(''['', @tableName) <> 0 OR \n             CHARINDEX('']'', @tableName) <> 0) \n             SET @canonTableName = @tableName \n         ELSE \n             SET @canonTableName = ''['' + @tableName + '']'' \n\n         /* First make sure the table exists */ \n         IF (SELECT OBJECT_ID(@tableName, ''U'')) IS NULL \n         BEGIN \n             RAISERROR (''00000001'', 16, 1) \n             RETURN \n         END \n\n         BEGIN TRAN\n         /* Insert the value into the notification table */ \n         IF NOT EXISTS (SELECT tableName FROM dbo.{0} WITH (NOLOCK) WHERE tableName = @tableName) \n             IF NOT EXISTS (SELECT tableName FROM dbo.{0} WITH (TABLOCKX) WHERE tableName = @tableName) \n                 INSERT  dbo.{0} \n                 VALUES (@tableName, GETDATE(), 0)\n\n         /* Create the trigger */ \n         SET @quotedTableName = QUOTENAME(@tableName, '''''''') \n         IF NOT EXISTS (SELECT name FROM sysobjects WITH (NOLOCK) WHERE name = @triggerName AND type = ''TR'') \n             IF NOT EXISTS (SELECT name FROM sysobjects WITH (TABLOCKX) WHERE name = @triggerName AND type = ''TR'') \n                 EXEC(''CREATE TRIGGER '' + @fullTriggerName + '' ON '' + @canonTableName +''\n                       FOR INSERT, UPDATE, DELETE AS BEGIN\n                       SET NOCOUNT ON\n                       EXEC dbo.{6} N'' + @quotedTableName + ''\n                       END\n                       '')\n         COMMIT TRAN\n         END\n   ')\n\n/* Create SP for updating the change Id of a table. */ \nIF NOT EXISTS (SELECT name FROM sysobjects WITH (NOLOCK) WHERE name = '{6}' AND type = 'P') \n   IF NOT EXISTS (SELECT name FROM sysobjects WITH (TABLOCKX) WHERE name = '{6}' AND type = 'P') \n   EXEC('CREATE PROCEDURE dbo.{6} \n             @tableName NVARCHAR(450) \n         AS\n\n         BEGIN \n             UPDATE dbo.{0} WITH (ROWLOCK) SET changeId = changeId + 1 \n             WHERE tableName = @tableName\n         END\n   ')\n\n/* Create SP for unregistering a table. */ \nIF NOT EXISTS (SELECT name FROM sysobjects WITH (NOLOCK) WHERE name = '{4}' AND type = 'P') \n   IF NOT EXISTS (SELECT name FROM sysobjects WITH (TABLOCKX) WHERE name = '{4}' AND type = 'P') \n   EXEC('CREATE PROCEDURE dbo.{4} \n             @tableName NVARCHAR(450) \n         AS\n         BEGIN\n\n         BEGIN TRAN\n         DECLARE @triggerName AS NVARCHAR(3000) \n         DECLARE @fullTriggerName AS NVARCHAR(3000)\n         SET @triggerName = REPLACE(@tableName, ''['', ''__o__'') \n         SET @triggerName = REPLACE(@triggerName, '']'', ''__c__'') \n         SET @triggerName = @triggerName + ''{3}'' \n         SET @fullTriggerName = ''dbo.['' + @triggerName + '']'' \n\n         /* Remove the table-row from the notification table */ \n         IF EXISTS (SELECT name FROM sysobjects WITH (NOLOCK) WHERE name = ''{0}'' AND type = ''U'') \n             IF EXISTS (SELECT name FROM sysobjects WITH (TABLOCKX) WHERE name = ''{0}'' AND type = ''U'') \n             DELETE FROM dbo.{0} WHERE tableName = @tableName \n\n         /* Remove the trigger */ \n         IF EXISTS (SELECT name FROM sysobjects WITH (NOLOCK) WHERE name = @triggerName AND type = ''TR'') \n             IF EXISTS (SELECT name FROM sysobjects WITH (TABLOCKX) WHERE name = @triggerName AND type = ''TR'') \n             EXEC(''DROP TRIGGER '' + @fullTriggerName) \n\n         COMMIT TRAN\n         END\n   ')\n\n/* Create SP for querying all registered table */ \nIF NOT EXISTS (SELECT name FROM sysobjects WITH (NOLOCK) WHERE name = '{5}' AND type = 'P') \n   IF NOT EXISTS (SELECT name FROM sysobjects WITH (TABLOCKX) WHERE name = '{5}' AND type = 'P') \n   EXEC('CREATE PROCEDURE dbo.{5} \n         AS\n         SELECT tableName FROM dbo.{0}   ')\n\n/* Create roles and grant them access to SP  */ \nIF NOT EXISTS (SELECT name FROM sysusers WHERE issqlrole = 1 AND name = N'aspnet_ChangeNotification_ReceiveNotificationsOnlyAccess') \n    EXEC sp_addrole N'aspnet_ChangeNotification_ReceiveNotificationsOnlyAccess' \n\nGRANT EXECUTE ON dbo.{1} to aspnet_ChangeNotification_ReceiveNotificationsOnlyAccess\n\n", new object[] { "AspNet_SqlCacheTablesForChangeNotification", "AspNet_SqlCachePollingStoredProcedure", "AspNet_SqlCacheRegisterTableStoredProcedure", "_AspNet_SqlCacheNotification_Trigger", "AspNet_SqlCacheUnRegisterTableStoredProcedure", "AspNet_SqlCacheQueryRegisteredTablesStoredProcedure", "AspNet_SqlCacheUpdateChangeIdStoredProcedure" });
                    command.CommandType = CommandType.Text;
                }
                else
                {
                    command.CommandText = string.Format(CultureInfo.InvariantCulture, "/* Remove notification table */ \nIF EXISTS (SELECT name FROM sysobjects WITH (NOLOCK) WHERE name = '{0}' AND type = 'U') \n    IF EXISTS (SELECT name FROM sysobjects WITH (TABLOCKX) WHERE name = '{0}' AND type = 'U') \n    BEGIN\n      /* First, unregister all registered tables */ \n      DECLARE tables_cursor CURSOR FOR \n      SELECT tableName FROM dbo.{0} \n      DECLARE @tableName AS NVARCHAR(450) \n\n      OPEN tables_cursor \n\n      /* Perform the first fetch. */ \n      FETCH NEXT FROM tables_cursor INTO @tableName \n\n      /* Check @@FETCH_STATUS to see if there are any more rows to fetch. */ \n      WHILE @@FETCH_STATUS = 0 \n      BEGIN \n          EXEC {3} @tableName \n\n          /* This is executed as long as the previous fetch succeeds. */ \n          FETCH NEXT FROM tables_cursor INTO @tableName \n      END \n      CLOSE tables_cursor \n      DEALLOCATE tables_cursor \n\n      /* Drop the table */\n      DROP TABLE dbo.{0} \n    END\n\n/* Remove polling SP */ \nIF EXISTS (SELECT name FROM sysobjects WITH (NOLOCK) WHERE name = '{1}' AND type = 'P') \n    IF EXISTS (SELECT name FROM sysobjects WITH (TABLOCKX) WHERE name = '{1}' AND type = 'P') \n      DROP PROCEDURE dbo.{1} \n\n/* Remove SP that registers a table */ \nIF EXISTS (SELECT name FROM sysobjects WITH (NOLOCK) WHERE name = '{2}' AND type = 'P') \n    IF EXISTS (SELECT name FROM sysobjects WITH (TABLOCKX) WHERE name = '{2}' AND type = 'P') \n      DROP PROCEDURE dbo.{2} \n\n/* Remove SP that unregisters a table */ \nIF EXISTS (SELECT name FROM sysobjects WITH (NOLOCK) WHERE name = '{3}' AND type = 'P') \n    IF EXISTS (SELECT name FROM sysobjects WITH (TABLOCKX) WHERE name = '{3}' AND type = 'P') \n      DROP PROCEDURE dbo.{3} \n\n/* Remove SP that querys the registered table */ \nIF EXISTS (SELECT name FROM sysobjects WITH (NOLOCK) WHERE name = '{4}' AND type = 'P') \n    IF EXISTS (SELECT name FROM sysobjects WITH (TABLOCKX) WHERE name = '{4}' AND type = 'P') \n      DROP PROCEDURE dbo.{4} \n\n/* Remove SP that updates the change Id of a table. */ \nIF EXISTS (SELECT name FROM sysobjects WITH (NOLOCK) WHERE name = '{5}' AND type = 'P') \n    IF EXISTS (SELECT name FROM sysobjects WITH (TABLOCKX) WHERE name = '{5}' AND type = 'P') \n      DROP PROCEDURE dbo.{5} \n\n/* Drop roles */ \nIF EXISTS ( SELECT name FROM sysusers WHERE issqlrole = 1 AND name = 'aspnet_ChangeNotification_ReceiveNotificationsOnlyAccess') BEGIN\nCREATE TABLE #aspnet_RoleMembers \n( \n    Group_name      sysname, \n    Group_id        smallint, \n    Users_in_group  sysname, \n    User_id         smallint \n) \nINSERT INTO #aspnet_RoleMembers \nEXEC sp_helpuser 'aspnet_ChangeNotification_ReceiveNotificationsOnlyAccess' \n \nDECLARE @user_id smallint \nDECLARE @cmd nvarchar(500) \nDECLARE c1 CURSOR FORWARD_ONLY FOR  \n    SELECT User_id FROM #aspnet_RoleMembers \n  \nOPEN c1 \n  \nFETCH c1 INTO @user_id \nWHILE (@@fetch_status = 0)  \nBEGIN \n    SET @cmd = 'EXEC sp_droprolemember ''aspnet_ChangeNotification_ReceiveNotificationsOnlyAccess'',''' + USER_NAME(@user_id) + '''' \n    EXEC (@cmd) \n    FETCH c1 INTO @user_id \nEND \n \nclose c1 \ndeallocate c1 \n    EXEC sp_droprole 'aspnet_ChangeNotification_ReceiveNotificationsOnlyAccess'\nEND\n", new object[] { "AspNet_SqlCacheTablesForChangeNotification", "AspNet_SqlCachePollingStoredProcedure", "AspNet_SqlCacheRegisterTableStoredProcedure", "AspNet_SqlCacheUnRegisterTableStoredProcedure", "AspNet_SqlCacheQueryRegisteredTablesStoredProcedure", "AspNet_SqlCacheUpdateChangeIdStoredProcedure" });
                    command.CommandType = CommandType.Text;
                }
                command.ExecuteNonQuery();
                command.CommandText = string.Empty;
                if (HttpRuntime.IsAspNetAppDomain)
                {
                    SqlCacheDependencyManager.UpdateAllDatabaseNotifState();
                }
            }
            catch (Exception exception)
            {
                string str2;
                SqlException exception2 = exception as SqlException;
                bool flag4 = true;
                if (exception2 != null)
                {
                    if (exception2.Number == 0xafc)
                    {
                        if (!flag2)
                        {
                            if (table != null)
                            {
                                throw new DatabaseNotEnabledForNotificationException(System.Web.SR.GetString("Database_not_enabled_for_notification", new object[] { connection.Database }));
                            }
                            throw;
                        }
                        if (table != null)
                        {
                            throw new DatabaseNotEnabledForNotificationException(System.Web.SR.GetString("Cant_disable_table_sql_cache_dep"));
                        }
                        flag4 = false;
                    }
                    else
                    {
                        if (((exception2.Number == 0xe5) || (exception2.Number == 0x106)) || ((exception2.Number == 0xac8) || (exception2.Number == 0x1205)))
                        {
                            string str;
                            if (!flag2)
                            {
                                if (table != null)
                                {
                                    str = "Permission_denied_table_enable_notification";
                                }
                                else
                                {
                                    str = "Permission_denied_database_enable_notification";
                                }
                            }
                            else if (table != null)
                            {
                                str = "Permission_denied_table_disable_notification";
                            }
                            else
                            {
                                str = "Permission_denied_database_disable_notification";
                            }
                            if (table != null)
                            {
                                throw new HttpException(System.Web.SR.GetString(str, new object[] { table }));
                            }
                            throw new HttpException(System.Web.SR.GetString(str));
                        }
                        if ((exception2.Number == 0xc350) && (exception2.Message == "00000001"))
                        {
                            throw new HttpException(System.Web.SR.GetString("Cache_dep_table_not_found", new object[] { table }));
                        }
                    }
                }
                if ((command != null) && (command.CommandText.Length != 0))
                {
                    str2 = System.Web.SR.GetString("Cant_connect_sql_cache_dep_database_admin_cmdtxt", new object[] { command.CommandText });
                }
                else
                {
                    str2 = System.Web.SR.GetString("Cant_connect_sql_cache_dep_database_admin");
                }
                if (flag4)
                {
                    throw new HttpException(str2, exception);
                }
            }
            finally
            {
                if (connection != null)
                {
                    connection.Close();
                }
            }
        }
    }
}

