SELECT name, type_desc
FROM sys.master_files
WHERE database_id = DB_ID('Mcareplus_AI') AND type_desc = 'LOG';

ALTER DATABASE [Mcareplus_AI] SET RECOVERY SIMPLE;

DBCC SHRINKFILE (N'<the name from step 1>', 512);
