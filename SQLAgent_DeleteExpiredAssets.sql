USE [msdb]
GO

/****** Object:  Job [RemoveExpiredAssets]    Script Date: 08/04/2020 15:01:24 ******/
EXEC msdb.dbo.sp_delete_job @job_id=N'bf35c32e-e3d0-4e65-85b1-c8b43c9e0f5f', @delete_unused_schedule=1
GO

/****** Object:  Job [RemoveExpiredAssets]    Script Date: 08/04/2020 15:01:24 ******/
BEGIN TRANSACTION
DECLARE @ReturnCode INT
SELECT @ReturnCode = 0
/****** Object:  JobCategory [[Uncategorized (Local)]]    Script Date: 08/04/2020 15:01:24 ******/
IF NOT EXISTS (SELECT name FROM msdb.dbo.syscategories WHERE name=N'[Uncategorized (Local)]' AND category_class=1)
BEGIN
EXEC @ReturnCode = msdb.dbo.sp_add_category @class=N'JOB', @type=N'LOCAL', @name=N'[Uncategorized (Local)]'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

END

DECLARE @jobId BINARY(16)
EXEC @ReturnCode =  msdb.dbo.sp_add_job @job_name=N'RemoveExpiredAssets', 
		@enabled=1, 
		@notify_level_eventlog=0, 
		@notify_level_email=0, 
		@notify_level_netsend=0, 
		@notify_level_page=0, 
		@delete_level=0, 
		@description=N'SQL Agent Job to remove All Expired assets from the ADI_Enrichment Database', 
		@category_name=N'[Uncategorized (Local)]', 
		@owner_login_name=N'gpusvr\Simon', @job_id = @jobId OUTPUT
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
/****** Object:  Step [Find And Delete Expired Assets]    Script Date: 08/04/2020 15:01:24 ******/
EXEC @ReturnCode = msdb.dbo.sp_add_jobstep @job_id=@jobId, @step_name=N'Find And Delete Expired Assets', 
		@step_id=1, 
		@cmdexec_success_code=0, 
		@on_success_action=1, 
		@on_success_step_id=0, 
		@on_fail_action=2, 
		@on_fail_step_id=0, 
		@retry_attempts=3, 
		@retry_interval=1, 
		@os_run_priority=0, @subsystem=N'TSQL', 
		@command=N'DECLARE @counter int
SET @counter = 0

DECLARE @HoursInPastToCheck int
SET @HoursInPastToCheck = -2

DECLARE @CheckTime DateTime
SET @CheckTime = (select DATEADD(hour, @HoursInPastToCheck ,GETDATE()))

DECLARE @IngestUUID uniqueidentifier, 
		@TITLPAID nvarchar(20), 
		@Licensing_Window_End nchar(50);
 
DECLARE @CURDT varchar(20)
DECLARE cursor_exipiry CURSOR

FOR SELECT 
        IngestUUID,
		TITLPAID,
		Licensing_Window_End
    FROM 
        Adi_Data
	WHERE
		Licensing_Window_End <= @CheckTime
 
OPEN cursor_exipiry;
 
FETCH NEXT FROM cursor_exipiry INTO 
    @IngestUUID, 
    @TITLPAID,
	@Licensing_Window_End;


WHILE @@FETCH_STATUS = 0
    BEGIN
		set @CURDT=CAST(SYSDATETIME() as varchar(30))
		RAISERROR(''%s - Asset with TITLPAID: %s has Expired with Expiry DateTime: %s'', 0,1, @CURDT, @TITLPAID, @Licensing_Window_End) WITH NOWAIT;
		RAISERROR(''%s - Removing TITLPAID: %s From the Database'', 0,1, @CURDT, @TITLPAID) WITH NOWAIT;
		
		DELETE FROM GN_Mapping_Data WHERE IngestUUID = @IngestUUID
		
		RAISERROR(''%s - Successfully DELETED TITLPAID: %s From the Database'', 0,1, @CURDT, @TITLPAID) WITH NOWAIT;
		SET @counter = @counter + 1

		FETCH NEXT FROM cursor_exipiry INTO 
			@IngestUUID, 
			@TITLPAID,
			@Licensing_Window_End;
    END;

RAISERROR(''%i Expired Assets deleted from the Database'', 0,1, @counter) WITH NOWAIT;
CLOSE cursor_exipiry;
 
DEALLOCATE cursor_exipiry;', 
		@database_name=N'ADI_Enrichment', 
		@output_file_name=N'D:\SQLAgentLog.txt', 
		@flags=22
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_update_job @job_id = @jobId, @start_step_id = 1
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_add_jobschedule @job_id=@jobId, @name=N'DeleteExpiredAssets', 
		@enabled=1, 
		@freq_type=4, 
		@freq_interval=1, 
		@freq_subday_type=4, 
		@freq_subday_interval=5, 
		@freq_relative_interval=0, 
		@freq_recurrence_factor=0, 
		@active_start_date=20200408, 
		@active_end_date=99991231, 
		@active_start_time=0, 
		@active_end_time=235959, 
		@schedule_uid=N'068f6116-43b5-493c-98f4-7d93070c4703'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_add_jobserver @job_id = @jobId, @server_name = N'(local)'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
COMMIT TRANSACTION
GOTO EndSave
QuitWithRollback:
    IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
EndSave:
GO


