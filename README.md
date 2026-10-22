Discovery	Nested objects / dynamic SQL	-	Not visible to ZAPSIGHTREPORTINGUSER - only the two audit tables are checked. Run as a DBA (way B) for the full check.
Procedure	dbo.usp_BenefitPlanInformation_Retrieve	EXECUTE	NOT FOUND
Procedure	dbo.USP_BenefitPlanInformation_Others_Retrieve	EXECUTE	NOT FOUND
Login	ZAPSIGHTREPORTINGUSER	exists	SQL_LOGIN (checks below run as this login)
Database access	Mcareplus_AI	CONNECT	OK
Procedure	dbo.USP_ClaimAI_SaveClaimBundle	EXECUTE	OK
Procedure	dbo.USP_ClaimMedicalScrutiny_Retrieve	EXECUTE	OK
Procedure	dbo.USP_ClaimHistory_Retrieve	EXECUTE	OK
Procedure	dbo.USP_BPSumInsured_Retrieve	EXECUTE	OK
Procedure	dbo.USP_LoadBPSIMasters	EXECUTE	OK
Procedure	dbo.SP_HideBufferDetails	EXECUTE	OK
Procedure	dbo.USP_GetBlockedAmount	EXECUTE	OK
Procedure	dbo.USP_familysuminsuredretrieve	EXECUTE	OK
Procedure	dbo.USP_GetHospitalpasthistory	EXECUTE	OK
Procedure	dbo.USP_ClaimsInwardRequestInfoByClaimID	EXECUTE	OK
Procedure	dbo.Usp_ClaimHospitalizationDetails	EXECUTE	OK
Procedure	dbo.Usp_ClaimRecPatientDetails	EXECUTE	OK
Procedure	dbo.Usp_ClaimSysPatientDetails	EXECUTE	OK
Procedure	dbo.Usp_Provider_Retrive	EXECUTE	OK
Procedure	dbo.USP_UCRData	EXECUTE	OK
Table	dbo.ClaimAI_Results	SELECT	OK
Table	dbo.ClaimAI_EventLog	SELECT	OK
Table	dbo.ClaimAI_SaveCount	SELECT	OK
Table	dbo.Claims	SELECT	OK
Table	dbo.Claimsdetails	SELECT	OK
Table	dbo.claimActionItems	SELECT	OK
Table	dbo.MemberPolicy	SELECT	OK
Table	dbo.MemberSI	SELECT	OK
Table	dbo.MemberUtilization	SELECT	OK
Table	dbo.BPSumInsured	SELECT	OK
Table	dbo.Policy	SELECT	OK
Table	dbo.Mst_AgeType	SELECT	OK
Table	dbo.Mst_Gender	SELECT	OK
Table	dbo.Mst_IssuingAuthority	SELECT	OK
Table	dbo.Mst_RelationShip	SELECT	OK
Table	dbo.Mst_Services	SELECT	OK
Table type	dbo.ClaimBillDetails	EXECUTE	OK
Table type	dbo.ClaimDeductionDetails	EXECUTE	OK
Table type	dbo.ClaimsServiceDetails	EXECUTE	OK
Table type	dbo.ClaimsCoding	EXECUTE	OK
Other database	Mcareplus_Audit	CONNECT	OK
Other database table	Mcareplus_Audit.dbo.Claims	INSERT	OK
Other database table	Mcareplus_Audit.dbo.Claimsdetails	INSERT	OK
