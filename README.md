SELECT physical_device_name
FROM msdb.dbo.backupmediafamily bmf
JOIN msdb.dbo.backupset bs ON bmf.media_set_id = bs.media_set_id
WHERE bs.database_name = 'Mcareplus_AI'
ORDER BY bs.backup_finish_date DESC;

BACKUP LOG [Mcareplus_AI] TO DISK = 'D:\Backups\Mcareplus_AI_log.trn';
