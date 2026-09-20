create an EC2 instance in AWS with this zip file.
create a task in windows task scheduler for this instance with these conditions:
1. use Create Task, not Create Basic Task.
2. every 5 minutes, indefinitely
3. Start in: the exe's folder (else it won't find its config)

in settings tab -> Do not start a new instance
in general tab -> Run whether user is logged on or not
Use the account that can reach SQL Server, DMS, ClaimAI and S3.

What "account" means

A Windows user account that the task runs as. In Task Scheduler's General tab there's a "When running the task, use the following user account" field.

A domain service account	e.g. FHPL\svc-claimai-staging. Best — doesn't expire with a person, survives staff changes
