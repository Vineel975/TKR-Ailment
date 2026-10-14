<img width="999" height="511" alt="image" src="https://github.com/user-attachments/assets/a5c72c58-5ab0-45cb-962e-3b10851cb9c9" />


<?definition /*-------------------------------------------------------------------
 Sp Title				: USP_ClaimCoding_Insert
 Author					:
 Date Created			: 
 Description			: 
 -------------------------------------------------------------
 Modification History
 ------------------------------------------------------------
 Sr No		Date		Modified By					Reason
 1.         20-11-2025  Ramesh A                    SP3V-5247	Package Selection -Mandatory - Medical Adjudication
 2.			23-02-2026	Ramesh A					SP3V-5376	SAP : Voluntary Top Up coverage as part of base SI
 3.			14-04-2026	Ramesh A					SP3V-5376	SAP : Voluntary Top Up coverage as part of base SI
 4.	        07-05-2026   subbu                      billing NME amount added
 -------------------------------------------------------------------*/
CREATE procedure [dbo].[USP_ClaimCoding_Insert]
(
	@ClaimID bigint,@Slno tinyint,@BillingType_P51 int,@Table ClaimsCoding readonly,@CreatedUserRegionID int,
@PackageType VARCHAR(2)=NULL,@NME_AMOUNT MONEY = NULL,
@Message varchar(100) OUTPUT
)    
AS    
BEGIN    
    
SET NOCOUNT ON    
    
 Update CLAIMSCODING set deleted=1,DeletedDatetime=getdate(),DeletedUserRegionID=@CreatedUserRegionID where  claimid=@claimid and slno=@slno    

 DECLARE @BILLING_EligibleAmount MONEY,@CODING_EligibleAmount MONEY,@PolicySublimit MONEY



--IF(@BILLING_EligibleAmount < @CODING_EligibleAmount)
--BEGIN
--	SET @NME_AMOUNT = (SELECT CASE WHEN @BILLING_EligibleAmount+@NME_AMOUNT < @PolicySublimit THEN @NME_AMOUNT
--								  WHEN @BILLING_EligibleAmount+@NME_AMOUNT >= @PolicySublimit THEN (@BILLING_EligibleAmount+@NME_AMOUNT)-@PolicySublimit  END )

--END

--IF(@BILLING_EligibleAmount > @CODING_EligibleAmount)
--BEGIN
--	SET @NME_AMOUNT = (SELECT CASE WHEN @CODING_EligibleAmount+@NME_AMOUNT < @PolicySublimit THEN @NME_AMOUNT
--								  WHEN @CODING_EligibleAmount+@NME_AMOUNT >= @PolicySublimit THEN (@CODING_EligibleAmount+@NME_AMOUNT )- @PolicySublimit END )

--END




 
 Insert into CLAIMSCODING (ClaimID,Slno,TPALevel1,TPALevel2,TPALevel3,BillingType_P51,TPAProcedureID,PackageRate,PackageRatio,AdditionalAmount,TreatementTypeID_19,isGipsa,isDayCare,isCI,isPED,TypeOfAnesthesiaID,Exclusions,SurgeryDate,BillAmount,    
 DisallowedAmount,DisallowedReasonIDs,EligibleAmount,PayableAmount,BufferAmount,AdditionalreasonIDs,Discount,Copay,Remarks,ICDCode,PCSCode,PCSDescription,Deleted,CreatedUserRegionID,Createddatetime,    
 DeletedUserRegionID,DeletedDatetime,ProcessHTML,BPCoverageLimit,Overridepackage,Overridesuminsured,PolicySublimit,AlimentExpression,Alimentpower,PackageType,NME_Amount,Billing_NME_Amount)   
 select @ClaimID,@Slno,TPALevel1,TPALevel2,TPALevel3,@BillingType_P51,TPAProcedureID,PackageRate,PackageRatio,AdditionalAmount,TreatementTypeID_19,isGipsa,isDayCare,isCI,isPED,TypeOfAnesthesiaID,Exclusions,SurgeryDate,BillAmount    
 ,(case when( DisallowedAmount is null or DisallowedAmount=0) then (isnull(BillAmount,0)-isnull(EligibleAmount,0)-isnull(Discount,0)) else DisallowedAmount end )DisallowedAmount,DisallowedReasonIDs,EligibleAmount,PayableAmount,BufferAmount,AdditionalreasonIDs,Discount,Copay,Remarks,ICDCode,PCSCode,PCSDescription,0,@CreatedUserRegionID    
 ,getdate(),NULL,NULL,ProcessHTML,BPCoverageLimit,Overridepackage,Overridesuminsured,PolicySublimit,AlimentExpression,Alimentpower ,@PackageType,@NME_AMOUNT,@NME_AMOUNT 
 from @Table    

if (exists(Select T.ICDCode from @Table T where T.IcdCode=107441))    
begin    
  insert into Mcareplus_Audit..Claimsdetails    
  Select *,getdate() from Claimsdetails with(nolock)   where ClaimID=@ClaimID and Slno=@Slno and Deleted=0      
  Update Claimsdetails Set IsCovid=1,IsRecalculated=0,ModifiedDatetime=getdate(),ModifiedUserRegionID=@CreatedUserRegionID  where ClaimID=@ClaimID And Slno=@Slno And Deleted=0    
 end    
else    
begin    
  insert into Mcareplus_Audit..Claimsdetails    
  Select *,getdate() from Claimsdetails with(nolock)  where ClaimID=@ClaimID and Slno=@Slno and Deleted=0     
  Update Claimsdetails Set IsCovid=0,IsRecalculated=0,ModifiedDatetime=getdate(),ModifiedUserRegionID=@CreatedUserRegionID  where ClaimID=@ClaimID And Slno=@Slno And Deleted=0    
 end
  ---added by VSVSkprasad refernce to production issue FHPL-AAAE-0549? Date 01-march-2023
 update Claimsdetails set IsRecalculated=1 where   ClaimID=@ClaimID and Slno=@Slno and Deleted=0  and IsoutofSI=1 and RequestTypeID in (4,12)
 
 Declare @Billamount money=0,@Eligibleamount money=0,@Deductionamount money=0,@Billamount1 money=0,@Eligibleamount1 money=0,@Deductionamount1 money=0,@DiscountAmount money=0    
 ,@DiscountAmount1 money=0 ,@PackageAmount money=0  
     
 --if(@BillingType_P51 in(202,203))    
 --select @Billamount=sum(billamount),@Deductionamount=isnull(sum(deductionamount),0),@Eligibleamount=isnull(sum(eligibleamount),0)    
 --from ClaimsServiceDetails where claimid=@ClaimID and Slno=@Slno and Deleted=0    
     
 --if(@BillingType_P51 in(201,203))    
 --select @Billamount1=sum(Billamount),@Deductionamount1=isnull(sum(DisallowedAmount),0),@Eligibleamount1=isnull(sum(PayableAmount),0)+isnull(sum(AdditionalAmount),0)    
 --from Claimscoding where claimid=@Claimid and slno=@Slno and deleted=0    
  IF((exists (Select 1 from Claimsdetails where ClaimID=@ClaimID and Slno=@slno and ISNULL(PackageAmount,0) !=0 and Deleted=0)) and (exists(Select 1 from ClaimsServiceDetails where ClaimID=@ClaimID and Slno=@slno and ISNULL(BillAmount,0)!=0 and Deleted=0)))
 SET @BillingType_P51 =203
 ELSE IF ((exists (Select 1 from Claimsdetails where ClaimID=@ClaimID and Slno=@slno and ISNULL(PackageAmount,0) =0 and Deleted=0)) and (exists(Select 1 from ClaimsServiceDetails where ClaimID=@ClaimID and Slno=@slno and ISNULL(BillAmount,0)!=0 and Deleted=0)))
 SET @BillingType_P51 =202
 ELSE IF ((exists (Select 1 from Claimsdetails where ClaimID=@ClaimID and Slno=@slno and ISNULL(PackageAmount,0) !=0 and Deleted=0)) and (Not exists(Select 1 from ClaimsServiceDetails where ClaimID=@ClaimID and Slno=@slno and ISNULL(BillAmount,0)!=0 and Deleted=0)))
 SET @BillingType_P51 =201   
     
 --if(@BillingType_P51 in(203))    
 select @Billamount=sum(billamount),@Deductionamount=isnull(sum(deductionamount),0),@Eligibleamount=isnull(sum(eligibleamount),0),@DiscountAmount=isnull(sum(DiscountAmount),0)    
 from ClaimsServiceDetails with(nolock) where claimid=@ClaimID and Slno=@Slno and Deleted=0    
     
 --if(@BillingType_P51 in(201,202,203))    
 select @Billamount1=sum(Billamount),@Deductionamount1=isnull(sum(DisallowedAmount),0),@Eligibleamount1=isnull(sum(PayableAmount),0)+isnull(sum(AdditionalAmount),0)    
 ,@DiscountAmount1=isnull(sum(Discount),0)    
 from Claimscoding with(nolock) where claimid=@Claimid and slno=@Slno and deleted=0    
  -- Package and Tariff  
 if(@BillingType_P51 in(203))  
 Begin
 	insert into Mcareplus_Audit..Claimsdetails
	Select *,getdate() from Claimsdetails with(nolock)  where ClaimID=@ClaimID and Slno=@Slno and Deleted=0   
	 Update Claimsdetails set BillAmount=@BillAmount+@Billamount1,EligibleAmount=@EligibleAmount+@EligibleAmount1,DeductionAmount=@Deductionamount+@Deductionamount1,    
   MOUDiscount=@DiscountAmount+@DiscountAmount1    
   ,BillingType_P51=@BillingType_P51,BillingCorrection=2--,PackageLimit=@PackageLimit,TariffValue=@TariffValue,BPCoverageLimit=@BPCoverageLimit     
   where ClaimID=@ClaimID and Slno=@Slno and Deleted=0    
 End   
  -- Only Package  
 else if(@BillingType_P51 in(201))  
  Begin  
   SET @PackageAmount=(select PackageAmount from ClaimsDetails with(nolock) where ClaimID=@ClaimID and Slno=@Slno and Deleted=0 )  
     
  if(@PackageAmount<@Eligibleamount1) 
  begin
   	insert into Mcareplus_Audit..Claimsdetails
	Select *,getdate() from Claimsdetails with(nolock)  where ClaimID=@ClaimID and Slno=@Slno and Deleted=0     
   Update Claimsdetails set BillAmount=@BillAmount,EligibleAmount=@PackageAmount,DeductionAmount=0,moudiscount=0  
   ,BillingType_P51=@BillingType_P51,BillingCorrection=2 ----,PackageLimit=@PackageLimit,TariffValue=@TariffValue,BPCoverageLimit=@BPCoverageLimit   
   where ClaimID=@ClaimID and Slno=@Slno and Deleted=0     
   end
	else 
	begin
	 	insert into Mcareplus_Audit..Claimsdetails
	Select *,getdate() from Claimsdetails with(nolock)  where ClaimID=@ClaimID and Slno=@Slno and Deleted=0     
 
   Update Claimsdetails set BillAmount=@Billamount1,EligibleAmount=@Eligibleamount1,DeductionAmount=@Deductionamount1,MOUDiscount=@DiscountAmount1,  
   BillingType_P51=@BillingType_P51,BillingCorrection=2--,PackageLimit=@PackageLimit,TariffValue=@TariffValue,BPCoverageLimit=@BPCoverageLimit   
   where ClaimID=@ClaimID and Slno=@Slno and Deleted=0  
	End 
	end 
   -- Only Tariff  
 else if(@BillingType_P51 in(202))  
 Begin     
  if(@Eligibleamount<=@Eligibleamount1)
  Begin
  	insert into Mcareplus_Audit..Claimsdetails
	Select *,getdate() from Claimsdetails  with(nolock) where ClaimID=@ClaimID and Slno=@Slno and Deleted=0     
     
   Update Claimsdetails set BillAmount=@BillAmount,EligibleAmount=@EligibleAmount,DeductionAmount=@Deductionamount,MOUDiscount=@DiscountAmount,  
   BillingType_P51=@BillingType_P51,BillingCorrection=2 ----,PackageLimit=@PackageLimit,TariffValue=@TariffValue,BPCoverageLimit=@BPCoverageLimit   
   where ClaimID=@ClaimID and Slno=@Slno and Deleted=0  
   end
  else
  Begin
  	insert into Mcareplus_Audit..Claimsdetails
	Select *,getdate() from Claimsdetails  with(nolock)  where ClaimID=@ClaimID and Slno=@Slno and Deleted=0     
   
   Update Claimsdetails set BillAmount=@Billamount1,EligibleAmount=@Eligibleamount1,DeductionAmount=@Deductionamount1,MOUDiscount=@DiscountAmount1,  
   BillingType_P51=@BillingType_P51,BillingCorrection=2--,PackageLimit=@PackageLimit,TariffValue=@TariffValue,BPCoverageLimit=@BPCoverageLimit   
   where ClaimID=@ClaimID and Slno=@Slno and Deleted=0   
   End
 End  

 declare @InvestigationID int ,@providerstatusid int, @pincode  varchar(6),@providerID bigint, @Age int
Select @Age=Age,@providerId=providerid,@pincode=pincode from Claims cc,Provider PP where Cc.ProviderID=PP.ID and cc.ID=@ClaimID and cc.Deleted=0 and PP.Deleted=0 order BY cc.ID desc
set @providerstatusid=(select top 1 providerstatusid from ProviderCategory where ProviderID=@providerid and deleted=0 order by id desc) 
 
update claimInvestigationScore set deleted=1 from claimInvestigationScore ci,InvestigationScore Ins   where ci.InvestigationID= ins.id and claimid=@claimid and slno=@slno and conditionid=4 and ci.Deleted=0
 Insert Into claimInvestigationScore(claimid,slno,InvestigationID,createddatetime,createdregionID,modifiedby,modifieddatetime,deleted)
Select top 1 ClaimID,Slno,ins.ID,getdate(),@CreatedUserRegionID,null,null,0 from Claimscoding cc with(nolock),InvestigationScore ins with(nolock), icd10 II where cc.icdCode=ii.ID and ii.DiseaseCode=Ins.Name and claimID=@ClaimID and Slno=@Slno and cc.Deleted=0
 --if ((Select count(*) from claimInvestigationScore where claimid=@claimid and slno=@slno and deleted=0) <=1)
 --begin
update claimInvestigationScore set deleted=1 from claimInvestigationScore ci,InvestigationScore Ins with(nolock)   where ci.InvestigationID= ins.id and claimid=@claimid and slno=@slno and conditionid=1 and ci.Deleted=0
If exists(Select Name from investigationScore with(nolock) where name=@pincode and conditionid=1) 
begin
set @InvestigationID=(select top 1  id from investigationScore where name=@pincode and conditionid=1 and deleted=0 order by id desc)
insert into claimInvestigationScore(claimid,slno,InvestigationID,createddatetime,createdregionID,modifiedby,modifieddatetime,deleted)
values(@claimid,@slno,@InvestigationID,getdate(),1,null,null,0)
end

update claimInvestigationScore set deleted=1 from claimInvestigationScore ci,InvestigationScore Ins with(nolock)   where ci.InvestigationID= ins.id and claimid=@claimid and slno=@slno and conditionid=3 and ci.Deleted=0
if(@providerstatusid=5)
begin 
insert into claimInvestigationScore(claimid,slno,InvestigationID,createddatetime,createdregionID,modifiedby,modifieddatetime,deleted)
values(@claimid,@slno,1988,getdate(),1,null,null,0)
end 
else if(@providerstatusid=1)
begin 
insert into claimInvestigationScore(claimid,slno,InvestigationID,createddatetime,createdregionID,modifiedby,modifieddatetime,deleted)
values(@claimid,@slno,1987,getdate(),1,null,null,0)
end 

update claimInvestigationScore set deleted=1 from claimInvestigationScore ci,InvestigationScore Ins with(nolock)  where ci.InvestigationID= ins.id and claimid=@claimid and slno=@slno and conditionid=2 and ci.Deleted=0
if(@Age>18 and @Age<35)
begin
insert into claimInvestigationScore(claimid,slno,InvestigationID,createddatetime,createdregionID,modifiedby,modifieddatetime,deleted)
values(@claimid,@slno,1984,getdate(),1,null,null,0)
end
else if (@Age> 35 and @Age<40)
begin
insert into claimInvestigationScore(claimid,slno,InvestigationID,createddatetime,createdregionID,modifiedby,modifieddatetime,deleted)
values(@claimid,@slno,1985,getdate(),1,null,null,0)
end
else if(@Age>40 and @Age<45)
begin
insert into claimInvestigationScore(claimid,slno,InvestigationID,createddatetime,createdregionID,modifiedby,modifieddatetime,deleted)
values(@claimid,@slno,1986,getdate(),1,null,null,0)
end


 --SELECT @PolicySublimit = PolicySublimit FROM @Table

 SELECT @CODING_EligibleAmount = EligibleAmount,@PolicySublimit = PolicySublimit FROM CLAIMSCODING (NOLOCK) WHERE CLAIMID = @ClaimID AND SLNO = @Slno  AND DELETED = 0

 SELECT @BILLING_EligibleAmount = EligibleAmount FROM CLAIMSDETAILS (NOLOCK) WHERE CLAIMID = @ClaimID AND SLNO = @Slno  AND DELETED = 0

IF(@CODING_EligibleAmount > 0 AND @BILLING_EligibleAmount > 0)
BEGIN
	IF((@BILLING_EligibleAmount+@NME_AMOUNT)> @CODING_EligibleAmount)
	BEGIN
		IF(@CODING_EligibleAmount = @BILLING_EligibleAmount)
			SET @NME_AMOUNT = 0
		ELSE IF (@CODING_EligibleAmount > @BILLING_EligibleAmount)
			SET @NME_AMOUNT = @CODING_EligibleAmount - @BILLING_EligibleAmount
	END
END

UPDATE CLAIMSCODING SET NME_Amount =  @NME_AMOUNT WHERE CLAIMID = @ClaimID AND SLNO = @Slno AND DELETED = 0
--end
 Set @Message='Saved Successfully'    
    
end 

?>
