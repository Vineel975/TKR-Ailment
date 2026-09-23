using DocumentFormat.OpenXml.Drawing;
using Enrollment.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Utilities;
//using DAL; 

namespace Enrollment.ViewModel
{
    public class ProviderViewModel : Controller
    {
        DBHelper.DBHelper vDBHelper = null;

        public ProviderViewModel()
        {
            vDBHelper = new DBHelper.DBHelper(System.Configuration.ConfigurationManager.AppSettings["sqlConMCarePlus"].ToString());
        }

        #region Hospital Empanelment

        #region For Hospital registration form load
        public DataSet GetMasters()
        {
            DataSet ds = null;
            try
            {
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "USP_PR_LOADEMPANELMASTER");
                ds = vDBHelper.Execute();
                return ds;
            }
            catch (Exception Ex)
            {
                string errorMsg = Ex.Message;
                return ds;
            }
        }
        #endregion

        #region For Districts Master
        public DataTable GetDistricts(string stateID)
        {
            DataTable dt = null;
            try
            {
                vDBHelper.mCreateCommand(CommandType.Text, "Select ID, Name from Mst_District where Stateid=@stateID order by Name");
                vDBHelper.mAddParameter("@stateID", SqlDbType.Int, ParameterDirection.Input, stateID);
                dt = vDBHelper.ExecuteDT();
                return dt;
            }
            catch (Exception Ex)
            {
                string errorMsg = Ex.Message;
                return dt;
            }
        }
        #endregion

        #region For Cities Master. Params-DistrictID
        public DataTable GetCities(string DistrictID, string type)
        {
            DataTable dt = null;
            try
            {
                if (type == "D")
                    vDBHelper.mCreateCommand(CommandType.Text, "Select ID, Name from Mst_City where Districtid=@DistrictID order by Name");
                else
                    vDBHelper.mCreateCommand(CommandType.Text, " Select ID, Name from Mst_City where Stateid=@DistrictID order by Name");
                vDBHelper.mAddParameter("@DistrictID", SqlDbType.Int, ParameterDirection.Input, DistrictID);
                dt = vDBHelper.ExecuteDT();
                return dt;
            }
            catch (Exception Ex)
            {
                string errorMsg = Ex.Message;
                throw Ex;
            }
        }
        #endregion

        #region For Hospital Creation
        public DataSet CreateHospital(HospitalEnpanelment hosp_details, out string strreturn, out long ProviderReqID)
        {
            try
            {
                // var ravenClient = new RavenClient("https://<key>:<secret>@app.getsentry.com/<project>");
                DataTable dt = new DataTable();
                dt.Columns.Add("FacilityID", typeof(int));
                dt.Columns.Add("DisplayName", typeof(string));
                dt.Columns.Add("FacilityValue", typeof(string));
                dt.Columns.Add("DMSID", typeof(int));
                dt.Columns.Add("Percentage", typeof(decimal));

                foreach (RoomTypes rtype in hosp_details.rt)
                {
                    dt.Rows.Add(rtype.FacilityID, rtype.DisplayName, rtype.FacilityValue, rtype.DMSID, rtype.Percentage);
                }

                DataSet ds = null;
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "USP_PR_CreateRequest");
                vDBHelper.mAddParameter("@Name", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.Name);
                vDBHelper.mAddParameter("@ShortName", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.ShortName);
                vDBHelper.mAddParameter("@ServiceTAXNo", SqlDbType.VarChar, int.MaxValue, ParameterDirection.Input, hosp_details.ServiceTAXNo);
                vDBHelper.mAddParameter("@ServiceTAXNoDMSID", SqlDbType.Int, ParameterDirection.Input, hosp_details.ServiceTAXNoDMSID);

                vDBHelper.mAddParameter("@PANNo", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.PANNo);
                vDBHelper.mAddParameter("@PANNumberDMSID", SqlDbType.Int, ParameterDirection.Input, hosp_details.PANNumberDMSID);
                vDBHelper.mAddParameter("@RegistrationNo", SqlDbType.VarChar, int.MaxValue, ParameterDirection.Input, hosp_details.RegistrationNo);
                vDBHelper.mAddParameter("@RegistrationNoDMSID", SqlDbType.Int, ParameterDirection.Input, hosp_details.RegistrationNoDMSID);

                vDBHelper.mAddParameter("@YearofEstablishment", SqlDbType.Int, ParameterDirection.Input, hosp_details.YearofEstablishment);
                vDBHelper.mAddParameter("@PayeeName", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.PayeeName);
                vDBHelper.mAddParameter("@OwnershipID_P35", SqlDbType.Int, int.MaxValue, ParameterDirection.Input, hosp_details.OwnershipID_P35);
                vDBHelper.mAddParameter("@OwnershipOthers", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.OwnershipOthers);

                vDBHelper.mAddParameter("@Recognizedby_P36", SqlDbType.Int, ParameterDirection.Input, hosp_details.Recognizedby_P36);
                vDBHelper.mAddParameter("@RecognizedbyOthers", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.RecognizedbyOthers);
                vDBHelper.mAddParameter("@HospitalType_P37", SqlDbType.Int, int.MaxValue, ParameterDirection.Input, hosp_details.HospitalType_P37);
                vDBHelper.mAddParameter("@SpecialtyType_P38", SqlDbType.Int, ParameterDirection.Input, hosp_details.SpecialtyType_P38);

                vDBHelper.mAddParameter("@RohiniCode", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.RohiniCode);
                vDBHelper.mAddParameter("@RohiniDate", SqlDbType.DateTime, ParameterDirection.Input, hosp_details.RohiniDate);

                vDBHelper.mAddParameter("@Accreditation_P39", SqlDbType.Int, ParameterDirection.Input, hosp_details.Accreditation_P39);
                vDBHelper.mAddParameter("@TDSExemption", SqlDbType.Bit, ParameterDirection.Input, hosp_details.TDSExemption);
                vDBHelper.mAddParameter("@IRDACode", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.IRDACode);
                vDBHelper.mAddParameter("@CancelledChequeDMSID", SqlDbType.Int, ParameterDirection.Input, hosp_details.CancelledChequeDMSID);

                vDBHelper.mAddParameter("@Address1", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.Address1);
                vDBHelper.mAddParameter("@Address2", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.Address2);
                vDBHelper.mAddParameter("@StateID", SqlDbType.Int, int.MaxValue, ParameterDirection.Input, hosp_details.StateID);
                vDBHelper.mAddParameter("@DistrictID", SqlDbType.Int, ParameterDirection.Input, hosp_details.DistrictID);

                vDBHelper.mAddParameter("@CityID", SqlDbType.Int, ParameterDirection.Input, hosp_details.CityID);
                vDBHelper.mAddParameter("@CityOthers", SqlDbType.VarChar, ParameterDirection.Input, "");
                vDBHelper.mAddParameter("@Location", SqlDbType.VarChar, int.MaxValue, ParameterDirection.Input, hosp_details.Location);
                vDBHelper.mAddParameter("@PINCode", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.PINCode);

                vDBHelper.mAddParameter("@Logitude", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.Logitude);
                vDBHelper.mAddParameter("@latitude", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.latitude);
                vDBHelper.mAddParameter("@STDCode", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.STDCode);
                vDBHelper.mAddParameter("@Landline1", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.Landline1);
                vDBHelper.mAddParameter("@Landline2", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.Landline2);

                vDBHelper.mAddParameter("@MobileNo", SqlDbType.BigInt, ParameterDirection.Input, hosp_details.MobileNo);
                Int64 s = 0;
                if (Int64.TryParse(hosp_details.SecMobileNo, out s))
                    vDBHelper.mAddParameter("@SecMobileNo", SqlDbType.BigInt, ParameterDirection.Input, hosp_details.SecMobileNo);
                vDBHelper.mAddParameter("@Email", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.Email);
                vDBHelper.mAddParameter("@SecEmail", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.SecEmail);

                vDBHelper.mAddParameter("@FAXNo", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.FAXNo);
                vDBHelper.mAddParameter("@SecFAXNo", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.SecFAXNo);
                vDBHelper.mAddParameter("@Website", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.Website);
                vDBHelper.mAddParameter("@OwnerName", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.OwnerName);
                vDBHelper.mAddParameter("@HierarchyID", SqlDbType.Int, int.MaxValue, ParameterDirection.Input, hosp_details.HierarchyID);
                vDBHelper.mAddParameter("@OwnerDesignation_P40", SqlDbType.Int, ParameterDirection.Input, hosp_details.OwnerDesignation_P40);
                vDBHelper.mAddParameter("@OwnerMobile", SqlDbType.BigInt, ParameterDirection.Input, hosp_details.OwnerMobile);
                vDBHelper.mAddParameter("@OwnerEmail", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.OwnerEmail);
                vDBHelper.mAddParameter("@OwnerLandline", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.OwnerLandline);

                vDBHelper.mAddParameter("@OwnerFax", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.OwnerFax);
                vDBHelper.mAddParameter("@OwnerPhotoDMSID", SqlDbType.Int, ParameterDirection.Input, hosp_details.OwnerPhotoDMSID);
                vDBHelper.mAddParameter("@Tablevariable", SqlDbType.Structured, ParameterDirection.Input, dt);

                vDBHelper.mAddParameter("@CreatedUserRegionID", SqlDbType.Int, ParameterDirection.Input, hosp_details.CreatedUserRegionID);
                vDBHelper.mAddParameter("@IsByPatriate", SqlDbType.Bit, ParameterDirection.Input, hosp_details.IsByPatriate);
                vDBHelper.mAddParameter("@InsurerID", SqlDbType.Int, ParameterDirection.Input, hosp_details.InsurerID);
                vDBHelper.mAddParameter("@EmpanelSource", SqlDbType.Int, ParameterDirection.Input, 160);
                //vDBHelper.mAddParameter("@GIPSA", SqlDbType.Bit, ParameterDirection.Input, hosp_details.IsGIPSA);
                //vDBHelper.mAddParameter("@IsFHPLPPN", SqlDbType.Bit, ParameterDirection.Input, hosp_details.IsFHPLPPN);
                //vDBHelper.mAddParameter("@SOCEffectiveDate", SqlDbType.DateTime, ParameterDirection.Input,DBNull.Value);
                //vDBHelper.mAddParameter("@GSTIN", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.GSTIN);

                vDBHelper.mAddParameter("@ID", SqlDbType.Int, ParameterDirection.Output, 0);
                vDBHelper.mAddParameter("@Msg", SqlDbType.VarChar, 1000, ParameterDirection.Output, "");

                DataSet ds1 = vDBHelper.ExecuteandOutparams(out ProviderReqID, "@ID", out strreturn, "@Msg");
                return ds1;

            }
            catch (Exception ex)
            {
                string errorMsg = ex.Message;
                throw;
            }
        }

        public string UpdateHospital(HospitalEnpanelment hosp_details)
        {
            try
            {
                //DataSet ds = null;
                DataTable dt = new DataTable();
                dt.Columns.Add("FacilityID", typeof(int));
                dt.Columns.Add("DisplayName", typeof(string));
                dt.Columns.Add("FacilityValue", typeof(string));
                dt.Columns.Add("DMSID", typeof(int));
                dt.Columns.Add("Percentage", typeof(decimal));

                foreach (RoomTypes rtype in hosp_details.rt)
                {
                    dt.Rows.Add(rtype.FacilityID, rtype.DisplayName, rtype.FacilityValue, rtype.DMSID, rtype.Percentage);
                }

                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "USP_PR_UpdateProviderDetails_Network");
                vDBHelper.mAddParameter("@ID", SqlDbType.BigInt, ParameterDirection.Input, hosp_details.ID);
                vDBHelper.mAddParameter("@Name", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.Name);
                vDBHelper.mAddParameter("@ShortName", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.ShortName);
                vDBHelper.mAddParameter("@ServiceTAXNo", SqlDbType.VarChar, int.MaxValue, ParameterDirection.Input, hosp_details.ServiceTAXNo);
                vDBHelper.mAddParameter("@ServiceTAXNoDMSID", SqlDbType.Int, ParameterDirection.Input, hosp_details.ServiceTAXNoDMSID);

                vDBHelper.mAddParameter("@PANNo", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.PANNo);
                vDBHelper.mAddParameter("@PANNumberDMSID", SqlDbType.Int, ParameterDirection.Input, hosp_details.PANNumberDMSID);
                vDBHelper.mAddParameter("@RegistrationNo", SqlDbType.VarChar, int.MaxValue, ParameterDirection.Input, hosp_details.RegistrationNo);
                vDBHelper.mAddParameter("@RegistrationNoDMSID", SqlDbType.Int, ParameterDirection.Input, hosp_details.RegistrationNoDMSID);

                vDBHelper.mAddParameter("@YearofEstablishment", SqlDbType.Int, ParameterDirection.Input, hosp_details.YearofEstablishment);
                vDBHelper.mAddParameter("@PayeeName", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.PayeeName);
                vDBHelper.mAddParameter("@DeducteeName", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.DeducteeName);
                vDBHelper.mAddParameter("@OwnershipID_P35", SqlDbType.Int, int.MaxValue, ParameterDirection.Input, hosp_details.OwnershipID_P35);
                vDBHelper.mAddParameter("@OwnershipOthers", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.OwnershipOthers);

                vDBHelper.mAddParameter("@Recognizedby_P36", SqlDbType.Int, ParameterDirection.Input, hosp_details.Recognizedby_P36);
                vDBHelper.mAddParameter("@RecognizedbyOthers", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.RecognizedbyOthers);
                vDBHelper.mAddParameter("@HospitalType_P37", SqlDbType.Int, int.MaxValue, ParameterDirection.Input, hosp_details.HospitalType_P37);
                vDBHelper.mAddParameter("@SpecialtyType_P38", SqlDbType.Int, ParameterDirection.Input, hosp_details.SpecialtyType_P38);

                vDBHelper.mAddParameter("@Accreditation_P39", SqlDbType.Int, ParameterDirection.Input, hosp_details.Accreditation_P39);
                vDBHelper.mAddParameter("@TDSExemption", SqlDbType.Bit, ParameterDirection.Input, hosp_details.TDSExemption);
                vDBHelper.mAddParameter("@IRDACode", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.IRDACode);
                vDBHelper.mAddParameter("@RohiniCode", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.RohiniCode);
                vDBHelper.mAddParameter("@CancelledChequeDMSID", SqlDbType.Int, ParameterDirection.Input, hosp_details.CancelledChequeDMSID);

                vDBHelper.mAddParameter("@Address1", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.Address1);
                vDBHelper.mAddParameter("@Address2", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.Address2);
                vDBHelper.mAddParameter("@StateID", SqlDbType.Int, int.MaxValue, ParameterDirection.Input, hosp_details.StateID);
                vDBHelper.mAddParameter("@DistrictID", SqlDbType.Int, ParameterDirection.Input, hosp_details.DistrictID);

                vDBHelper.mAddParameter("@CityID", SqlDbType.Int, ParameterDirection.Input, hosp_details.CityID);
                vDBHelper.mAddParameter("@CityOthers", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.CityOthers);
                vDBHelper.mAddParameter("@Location", SqlDbType.VarChar, int.MaxValue, ParameterDirection.Input, hosp_details.Location);
                vDBHelper.mAddParameter("@PINCode", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.PINCode);

                vDBHelper.mAddParameter("@Logitude", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.Logitude);
                vDBHelper.mAddParameter("@latitude", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.latitude);
                vDBHelper.mAddParameter("@STDCode", SqlDbType.VarChar, int.MaxValue, ParameterDirection.Input, hosp_details.STDCode);
                vDBHelper.mAddParameter("@Landline1", SqlDbType.VarChar, int.MaxValue, ParameterDirection.Input, hosp_details.Landline1);
                vDBHelper.mAddParameter("@Landline2", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.Landline2);

                vDBHelper.mAddParameter("@MobileNo", SqlDbType.BigInt, ParameterDirection.Input, hosp_details.MobileNo);
                Int64 s = 0;
                if (Int64.TryParse(hosp_details.SecMobileNo, out s))
                    vDBHelper.mAddParameter("@SecMobileNo", SqlDbType.BigInt, ParameterDirection.Input, hosp_details.SecMobileNo);
                vDBHelper.mAddParameter("@Email", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.Email);
                vDBHelper.mAddParameter("@SecEmail", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.SecEmail);

                vDBHelper.mAddParameter("@FAXNo", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.FAXNo);
                vDBHelper.mAddParameter("@SecFAXNo", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.SecFAXNo);
                vDBHelper.mAddParameter("@Website", SqlDbType.VarChar, int.MaxValue, ParameterDirection.Input, hosp_details.Website);
                vDBHelper.mAddParameter("@OwnerName", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.OwnerName);

                vDBHelper.mAddParameter("@OwnerDesignation_P40", SqlDbType.Int, ParameterDirection.Input, hosp_details.OwnerDesignation_P40);
                if (hosp_details.OwnerMobile != "")
                    vDBHelper.mAddParameter("@OwnerMobile", SqlDbType.BigInt, ParameterDirection.Input, hosp_details.OwnerMobile);
                vDBHelper.mAddParameter("@OwnerEmail", SqlDbType.VarChar, int.MaxValue, ParameterDirection.Input, hosp_details.OwnerEmail);
                vDBHelper.mAddParameter("@OwnerLandline", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.OwnerLandline);

                vDBHelper.mAddParameter("@OwnerFax", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.OwnerFax);
                vDBHelper.mAddParameter("@OwnerPhotoDMSID", SqlDbType.Int, ParameterDirection.Input, hosp_details.OwnerPhotoDMSID);
                vDBHelper.mAddParameter("@HierarchyID", SqlDbType.Int, int.MaxValue, ParameterDirection.Input, hosp_details.HierarchyID);

                vDBHelper.mAddParameter("@Tablevariable", SqlDbType.Structured, ParameterDirection.Input, dt);

                vDBHelper.mAddParameter("@CreatedUserRegionID", SqlDbType.Int, ParameterDirection.Input, hosp_details.CreatedUserRegionID);
                vDBHelper.mAddParameter("@EmpanelSource", SqlDbType.Int, ParameterDirection.Input, 160);
                vDBHelper.mAddParameter("@HospitalCategoty", SqlDbType.Int, ParameterDirection.Input, hosp_details.HospitalCategory_P68);
                vDBHelper.mAddParameter("@TotalNoofBeds", SqlDbType.Int, ParameterDirection.Input, hosp_details.TotalNoofBeds);
                vDBHelper.mAddParameter("@Remarks", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.Remarks);
                vDBHelper.mAddParameter("@GIPSA", SqlDbType.Bit, ParameterDirection.Input, hosp_details.IsGIPSA);
                vDBHelper.mAddParameter("@IsFHPLPPN", SqlDbType.Bit, ParameterDirection.Input, hosp_details.IsFHPLPPN);
                vDBHelper.mAddParameter("@IsEmpanelment", SqlDbType.Bit, ParameterDirection.Input, hosp_details.IsEmpanelment);
                vDBHelper.mAddParameter("@SOCEffectiveDate", SqlDbType.DateTime, ParameterDirection.Input, hosp_details.SOCEffectiveDate);
                vDBHelper.mAddParameter("@GSTIN", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.GSTIN);
                vDBHelper.mAddParameter("@NABHAccreditionType", SqlDbType.Int, ParameterDirection.Input, hosp_details.NABhAccreditionType);
                vDBHelper.mAddParameter("@NABHCode", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.NABHCode);
                vDBHelper.mAddParameter("@RohiniDate", SqlDbType.DateTime, ParameterDirection.Input, hosp_details.RohiniDate);
                vDBHelper.mAddParameter("@OPServices", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.OPServices);

                vDBHelper.mAddParameter("@Empanelmentrequestreceivedfrom", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.Empanelmentrequestreceivedfrom);// added by vydehi
                vDBHelper.mAddParameter("@SPOC", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.SPOC);
                vDBHelper.mAddParameter("@SPOCmailID", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.SPOCmailID);
                vDBHelper.mAddParameter("@SPOCdate", SqlDbType.DateTime, ParameterDirection.Input, hosp_details.SPOCdate);
                vDBHelper.mAddParameter("@AccreditationExpiryDate", SqlDbType.DateTime, ParameterDirection.Input, hosp_details.AccreditationExpiryDate);// added by vydehi
                vDBHelper.mAddParameter("@NABHExpiry", SqlDbType.DateTime, ParameterDirection.Input, hosp_details.NABHExpiry);
                vDBHelper.mAddParameter("@RohiniExpiryDate", SqlDbType.DateTime, ParameterDirection.Input, hosp_details.RohiniExpiryDate);
                vDBHelper.mAddParameter("@RegisteredAuthority", SqlDbType.Int, ParameterDirection.Input, hosp_details.RegisteredAuthority);
                vDBHelper.mAddParameter("@RegistrationExpiryDate", SqlDbType.DateTime, ParameterDirection.Input, hosp_details.RegistrationExpiryDate);
                vDBHelper.mAddParameter("@GIPSAInceptionStartDate", SqlDbType.DateTime, ParameterDirection.Input, hosp_details.GIPSAInceptionStartDate);
                vDBHelper.mAddParameter("@GIPSAInceptionEndDate", SqlDbType.DateTime, ParameterDirection.Input, hosp_details.GIPSAInceptionEndDate);
                vDBHelper.mAddParameter("@FHPLInceptionStartDate", SqlDbType.DateTime, ParameterDirection.Input, hosp_details.FHPLInceptionStartDate);
                vDBHelper.mAddParameter("@FHPLInceptionEndDate", SqlDbType.DateTime, ParameterDirection.Input, hosp_details.FHPLInceptionEndDate);
                vDBHelper.mAddParameter("@GICInceptionDate", SqlDbType.DateTime, ParameterDirection.Input, hosp_details.GICInceptionDate);
                vDBHelper.mAddParameter("@Zone", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.Zone);
                //vDBHelper.mAddParameter("@Reason", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.Reason);
                vDBHelper.mAddParameter("@Msg", SqlDbType.VarChar, 1000, ParameterDirection.Output, "");

                string msg;
                vDBHelper.ExecuteNonQuery(out msg, "@Msg");
                return msg;
            }
            catch (Exception Ex)
            {
                string errormsg = Ex.Message;
                return "problem in updating your request";
                throw;
            }
        }

        public DataSet CreateHospital_NetworkUser(HospitalEnpanelment hosp_details, out string strreturn, out long ProviderReqID)
        {
            try
            {
                //DataSet ds = null;
                // var ravenClient = new RavenClient("https://<key>:<secret>@app.getsentry.com/<project>");
                DataTable dt = new DataTable();
                dt.Columns.Add("FacilityID", typeof(int));
                dt.Columns.Add("DisplayName", typeof(string));
                dt.Columns.Add("FacilityValue", typeof(string));
                dt.Columns.Add("DMSID", typeof(int));
                dt.Columns.Add("Percentage", typeof(decimal));

                foreach (RoomTypes rtype in hosp_details.rt)
                {
                    dt.Rows.Add(rtype.FacilityID, rtype.DisplayName, rtype.FacilityValue, rtype.DMSID, rtype.Percentage);
                }

                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "[USP_PR_CreateProvider]");
                vDBHelper.mAddParameter("@Name", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.Name);
                vDBHelper.mAddParameter("@ShortName", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.ShortName);
                vDBHelper.mAddParameter("@ServiceTAXNo", SqlDbType.VarChar, int.MaxValue, ParameterDirection.Input, hosp_details.ServiceTAXNo);
                vDBHelper.mAddParameter("@ServiceTAXNoDMSID", SqlDbType.Int, ParameterDirection.Input, hosp_details.ServiceTAXNoDMSID);

                vDBHelper.mAddParameter("@PANNo", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.PANNo);
                vDBHelper.mAddParameter("@PANNumberDMSID", SqlDbType.Int, ParameterDirection.Input, hosp_details.PANNumberDMSID);
                vDBHelper.mAddParameter("@RegistrationNo", SqlDbType.VarChar, int.MaxValue, ParameterDirection.Input, hosp_details.RegistrationNo);
                vDBHelper.mAddParameter("@RegistrationNoDMSID", SqlDbType.Int, ParameterDirection.Input, hosp_details.RegistrationNoDMSID);

                vDBHelper.mAddParameter("@YearofEstablishment", SqlDbType.Int, ParameterDirection.Input, hosp_details.YearofEstablishment);
                vDBHelper.mAddParameter("@PayeeName", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.PayeeName);
                vDBHelper.mAddParameter("@DeducteeName", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.DeducteeName);
                vDBHelper.mAddParameter("@OwnershipID_P35", SqlDbType.Int, int.MaxValue, ParameterDirection.Input, hosp_details.OwnershipID_P35);
                vDBHelper.mAddParameter("@OwnershipOthers", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.OwnershipOthers);

                vDBHelper.mAddParameter("@Recognizedby_P36", SqlDbType.Int, ParameterDirection.Input, hosp_details.Recognizedby_P36);
                vDBHelper.mAddParameter("@RecognizedbyOthers", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.RecognizedbyOthers);
                vDBHelper.mAddParameter("@HospitalType_P37", SqlDbType.Int, int.MaxValue, ParameterDirection.Input, hosp_details.HospitalType_P37);
                vDBHelper.mAddParameter("@SpecialtyType_P38", SqlDbType.Int, ParameterDirection.Input, hosp_details.SpecialtyType_P38);

                vDBHelper.mAddParameter("@Accreditation_P39", SqlDbType.Int, ParameterDirection.Input, hosp_details.Accreditation_P39);
                vDBHelper.mAddParameter("@TDSExemption", SqlDbType.Bit, ParameterDirection.Input, hosp_details.TDSExemption);
                vDBHelper.mAddParameter("@IRDACode", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.IRDACode);
                vDBHelper.mAddParameter("@RohiniCode", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.RohiniCode);
                vDBHelper.mAddParameter("@CancelledChequeDMSID", SqlDbType.Int, ParameterDirection.Input, hosp_details.CancelledChequeDMSID);

                vDBHelper.mAddParameter("@Address1", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.Address1);
                vDBHelper.mAddParameter("@Address2", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.Address2);
                vDBHelper.mAddParameter("@StateID", SqlDbType.Int, int.MaxValue, ParameterDirection.Input, hosp_details.StateID);
                vDBHelper.mAddParameter("@DistrictID", SqlDbType.Int, ParameterDirection.Input, hosp_details.DistrictID);

                vDBHelper.mAddParameter("@CityID", SqlDbType.Int, ParameterDirection.Input, hosp_details.CityID);
                vDBHelper.mAddParameter("@CityOthers", SqlDbType.VarChar, ParameterDirection.Input, "");
                vDBHelper.mAddParameter("@Location", SqlDbType.VarChar, int.MaxValue, ParameterDirection.Input, hosp_details.Location);
                vDBHelper.mAddParameter("@PINCode", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.PINCode);

                vDBHelper.mAddParameter("@Logitude", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.Logitude);
                vDBHelper.mAddParameter("@latitude", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.latitude);
                vDBHelper.mAddParameter("@STDCode", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.STDCode);
                vDBHelper.mAddParameter("@Landline1", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.Landline1);
                vDBHelper.mAddParameter("@Landline2", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.Landline2);

                Int64 s = 0;
                if (Int64.TryParse(hosp_details.MobileNo, out s))
                    vDBHelper.mAddParameter("@MobileNo", SqlDbType.BigInt, ParameterDirection.Input, hosp_details.MobileNo);

                if (Int64.TryParse(hosp_details.SecMobileNo, out s))
                    vDBHelper.mAddParameter("@SecMobileNo", SqlDbType.BigInt, ParameterDirection.Input, hosp_details.SecMobileNo);
                vDBHelper.mAddParameter("@Email", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.Email);
                vDBHelper.mAddParameter("@SecEmail", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.SecEmail);

                vDBHelper.mAddParameter("@FAXNo", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.FAXNo);
                vDBHelper.mAddParameter("@SecFAXNo", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.SecFAXNo);
                vDBHelper.mAddParameter("@Website", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.Website);
                vDBHelper.mAddParameter("@OwnerName", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.OwnerName);
                vDBHelper.mAddParameter("@HierarchyID", SqlDbType.Int, int.MaxValue, ParameterDirection.Input, hosp_details.HierarchyID);
                vDBHelper.mAddParameter("@OwnerDesignation_P40", SqlDbType.Int, ParameterDirection.Input, hosp_details.OwnerDesignation_P40);
                if (Int64.TryParse(hosp_details.OwnerMobile, out s))
                    vDBHelper.mAddParameter("@OwnerMobile", SqlDbType.BigInt, ParameterDirection.Input, hosp_details.OwnerMobile);
                vDBHelper.mAddParameter("@OwnerEmail", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.OwnerEmail);
                vDBHelper.mAddParameter("@OwnerLandline", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.OwnerLandline);

                vDBHelper.mAddParameter("@OwnerFax", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.OwnerFax);
                vDBHelper.mAddParameter("@OwnerPhotoDMSID", SqlDbType.Int, ParameterDirection.Input, hosp_details.OwnerPhotoDMSID);

                vDBHelper.mAddParameter("@Tablevariable", SqlDbType.Structured, ParameterDirection.Input, dt);

                vDBHelper.mAddParameter("@CreatedUserRegionID", SqlDbType.Int, ParameterDirection.Input, hosp_details.CreatedUserRegionID);
                vDBHelper.mAddParameter("@IsByPatriate", SqlDbType.Bit, ParameterDirection.Input, hosp_details.IsByPatriate);
                vDBHelper.mAddParameter("@InsurerID", SqlDbType.Int, ParameterDirection.Input, hosp_details.InsurerID);
                vDBHelper.mAddParameter("@EmpanelSource", SqlDbType.Int, ParameterDirection.Input, 160);

                vDBHelper.mAddParameter("@HospitalCategoty", SqlDbType.Int, ParameterDirection.Input, hosp_details.HospitalCategory_P68);
                vDBHelper.mAddParameter("@TotalNoofBeds", SqlDbType.Int, ParameterDirection.Input, hosp_details.TotalNoofBeds);
                vDBHelper.mAddParameter("@Remarks", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.Remarks);
                vDBHelper.mAddParameter("@GIPSA", SqlDbType.Bit, ParameterDirection.Input, hosp_details.IsGIPSA);
                vDBHelper.mAddParameter("@IsFHPLPPN", SqlDbType.Bit, ParameterDirection.Input, hosp_details.IsFHPLPPN);
                vDBHelper.mAddParameter("@SOCEffectiveDate", SqlDbType.DateTime, ParameterDirection.Input, hosp_details.SOCEffectiveDate);
                vDBHelper.mAddParameter("@ID", SqlDbType.Int, ParameterDirection.Output, 0);
                vDBHelper.mAddParameter("@GSTIN", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.GSTIN);
                vDBHelper.mAddParameter("@NABHAccreditionType", SqlDbType.Int, ParameterDirection.Input, hosp_details.NABhAccreditionType);
                vDBHelper.mAddParameter("@NABHCode", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.NABHCode);
                vDBHelper.mAddParameter("@RohiniDate", SqlDbType.DateTime, ParameterDirection.Input, hosp_details.RohiniDate);
                vDBHelper.mAddParameter("@OPServices", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.OPServices);
                vDBHelper.mAddParameter("@Empanelmentrequestreceivedfrom", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.Empanelmentrequestreceivedfrom);// added by vydehi
                vDBHelper.mAddParameter("@SPOC", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.SPOC);
                vDBHelper.mAddParameter("@SPOCmailID", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.SPOCmailID);
                vDBHelper.mAddParameter("@SPOCdate", SqlDbType.DateTime, ParameterDirection.Input, hosp_details.SPOCdate);
                vDBHelper.mAddParameter("@AccreditationExpiryDate", SqlDbType.DateTime, ParameterDirection.Input, hosp_details.AccreditationExpiryDate);
                vDBHelper.mAddParameter("@NABHExpiry", SqlDbType.DateTime, ParameterDirection.Input, hosp_details.NABHExpiry);
                vDBHelper.mAddParameter("@RohiniExpiryDate", SqlDbType.DateTime, ParameterDirection.Input, hosp_details.RohiniExpiryDate);
                vDBHelper.mAddParameter("@RegisteredAuthority", SqlDbType.Int, ParameterDirection.Input, hosp_details.RegisteredAuthority);
                vDBHelper.mAddParameter("@RegistrationExpiryDate", SqlDbType.DateTime, ParameterDirection.Input, hosp_details.RegistrationExpiryDate);
                vDBHelper.mAddParameter("@GIPSAInceptionStartDate", SqlDbType.DateTime, ParameterDirection.Input, hosp_details.GIPSAInceptionStartDate);
                vDBHelper.mAddParameter("@GIPSAInceptionEndDate", SqlDbType.DateTime, ParameterDirection.Input, hosp_details.GIPSAInceptionEndDate);
                vDBHelper.mAddParameter("@FHPLInceptionStartDate", SqlDbType.DateTime, ParameterDirection.Input, hosp_details.FHPLInceptionStartDate);
                vDBHelper.mAddParameter("@FHPLInceptionEndDate", SqlDbType.DateTime, ParameterDirection.Input, hosp_details.FHPLInceptionEndDate);
                vDBHelper.mAddParameter("@GICInceptionDate", SqlDbType.DateTime, ParameterDirection.Input, hosp_details.GICInceptionDate);
                vDBHelper.mAddParameter("@Zone", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.Zone);

                //vDBHelper.mAddParameter("@Reason", SqlDbType.VarChar, ParameterDirection.Input, hosp_details.Reason);

                vDBHelper.mAddParameter("@Msg", SqlDbType.VarChar, 1000, ParameterDirection.Output, "");

                DataSet ds1 = vDBHelper.ExecuteandOutparams(out ProviderReqID, "@ID", out strreturn, "@Msg");
                return ds1;
            }
            catch (Exception ex)
            {
                string exMessage = ex.Message;
                throw;
            }
        }

        public DataSet GetHospitalDeatils_NetworkUser(long ProviderID)
        {
            try
            {
                DataSet ds = null;
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "USP_PROVIDER_RETRIEVE_FOREDIT");
                vDBHelper.mAddParameter("@ID", SqlDbType.BigInt, ParameterDirection.Input, ProviderID);
                ds = vDBHelper.Execute();
                return ds;
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
        }
        #endregion

        #region Hospital Contact Details
        public string InsertHospitalContactDetails(HospContactDetails contacts)
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("Name", typeof(string));
                dt.Columns.Add("DesignationID", typeof(int));
                dt.Columns.Add("ContactDepartment", typeof(int));
                dt.Columns.Add("MobileNo", typeof(Int64));
                dt.Columns.Add("EmailID", typeof(string));
                dt.Columns.Add("Landline1", typeof(string));
                dt.Columns.Add("FAXNo", typeof(string));

                dt.Rows.Add(contacts.Name, contacts.DesignationID, contacts.ContactDepartment, Convert.ToInt64(contacts.MobileNo), contacts.EmailID, contacts.Landline1, contacts.FAXNo);

                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "USP_ProvReqContacts_Insert");
                vDBHelper.mAddParameter("@ProviderReqID", SqlDbType.BigInt, ParameterDirection.Input, contacts.ProviderReqID);
                vDBHelper.mAddParameter("@ProviderReqContacts", SqlDbType.Structured, ParameterDirection.Input, dt);
                vDBHelper.mAddParameter("@CreatedUserRegionID", SqlDbType.Int, ParameterDirection.Input, contacts.CreatedUserRegionID);
                vDBHelper.mAddParameter("@Msg", SqlDbType.VarChar, 1000, ParameterDirection.Output, "");
                string strReturn;
                vDBHelper.ExecuteNonQuery(out strReturn, "@msg");
                return strReturn;
            }
            catch (Exception Ex)
            {
                return "Failed" + Ex.Message;
                throw;
            }
        }

        public string contacts()
        {
            return "Success";
        }
        #endregion
        #endregion

        #region Verification Form

        #region For Verfication Form Load
        public DataSet GetVerificationData(string ProvReqID)
        {
            try
            {
                DataSet ds = null;
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "USP_PROVREQ_RETRIEVE");
                vDBHelper.mAddParameter("@ProvReqID", SqlDbType.BigInt, ParameterDirection.Input, ProvReqID);
                ds = vDBHelper.Execute();

                DataTable Infrastructure = new DataTable();
                Infrastructure.Columns.Add("ID");
                Infrastructure.Columns.Add("LevelName");
                Infrastructure.Columns.Add("FacilityValue");
                Infrastructure.Columns.Add("DisplayName");
                Infrastructure.Columns.Add("DMSID");
                Infrastructure.Columns.Add("Type");
                Infrastructure.Columns.Add("ddValues");

                DataTable InnerTable = new DataTable();
                InnerTable.Columns.Add("ID");
                InnerTable.Columns.Add("Level3");
                InnerTable.Columns.Add("ProvFacilityID");

                DataTable table1 = ds.Tables[1];
                DataRow flagRow = table1.Rows[0];
                string flag = "", ddvalues = "";
                for (int i = 0; i < table1.Rows.Count - 1; i++)
                {
                    DataRow dr = table1.Rows[i];
                    DataRow nextRow = table1.Rows[i + 1];
                    if (dr["Level1"].ToString() == "Civil and Medical Infrastructure-General" && dr["Level2"].ToString() != "")
                    {
                        if (dr["Level2"].ToString() == nextRow["Level2"].ToString())
                        {
                            InnerTable.Rows.Add(dr["ID"], dr["Level3"], dr["ProvFacilityID"]);
                            flag = "true";
                        }
                        else if (flag == "true")
                        {
                            InnerTable.Rows.Add(dr["ID"], dr["Level3"], dr["ProvFacilityID"]);
                            flag = "false";
                            ddvalues = Newtonsoft.Json.JsonConvert.SerializeObject(InnerTable);
                            InnerTable.Clear();
                            Infrastructure.Rows.Add(dr["ParentID"], dr["Level2"], dr["FacilityValue"], dr["DisplayName"], dr["DMSID"], "DropDown", ddvalues);
                        }
                        else
                        {
                            InnerTable.Clear();
                            Infrastructure.Rows.Add(dr["ID"], dr["Level2"], dr["FacilityValue"], dr["DisplayName"], dr["DMSID"], "TextBox");
                        }

                    }
                }
                ds.Tables.Add(Infrastructure);

                Infrastructure = new DataTable();
                Infrastructure.Columns.Add("ID");
                Infrastructure.Columns.Add("LevelName");
                Infrastructure.Columns.Add("FacilityValue");
                Infrastructure.Columns.Add("DisplayName");
                Infrastructure.Columns.Add("DMSID");
                Infrastructure.Columns.Add("Type");
                Infrastructure.Columns.Add("ddValues");

                InnerTable = new DataTable();
                InnerTable.Columns.Add("ID");
                InnerTable.Columns.Add("Level3");
                InnerTable.Columns.Add("ProvFacilityID");

                table1 = ds.Tables[1];
                flagRow = table1.Rows[0];
                flag = "";
                ddvalues = "";
                for (int i = 0; i < table1.Rows.Count - 1; i++)
                {
                    DataRow dr = table1.Rows[i];
                    DataRow nextRow = table1.Rows[i + 1];
                    if (dr["Level1"].ToString() == "Permits" && dr["Level2"].ToString() != "")
                    {
                        if (dr["Level2"].ToString() == nextRow["Level2"].ToString())
                        {
                            InnerTable.Rows.Add(dr["ID"], dr["Level3"], dr["ProvFacilityID"]);
                            flag = "true";
                        }
                        else if (flag == "true")
                        {
                            InnerTable.Rows.Add(dr["ID"], dr["Level3"], dr["ProvFacilityID"]);
                            flag = "false";
                            ddvalues = Newtonsoft.Json.JsonConvert.SerializeObject(InnerTable);
                            InnerTable.Clear();
                            Infrastructure.Rows.Add(dr["ParentID"], dr["Level2"], dr["FacilityValue"], dr["DisplayName"], dr["DMSID"], "DropDown", ddvalues);
                        }
                        else
                        {
                            InnerTable.Clear();
                            Infrastructure.Rows.Add(dr["ID"], dr["Level2"], dr["FacilityValue"], dr["DisplayName"], dr["DMSID"], "TextBox");
                        }
                    }
                }
                ds.Tables.Add(Infrastructure);

                if (Convert.ToInt64(ProvReqID) == 0)
                {
                    vDBHelper.mCreateCommand(CommandType.Text, "Select Packagefilecount = 0,Servicefilecount = 0;");
                    DataTable dtFileHistory = vDBHelper.ExecuteDT();
                    ds.Tables.Add(dtFileHistory);
                }
                else
                {
                    vDBHelper.mCreateCommand(CommandType.Text, "Select Packagefilecount=count(case when FileStage=0 then id end ),Servicefilecount=count(case when FileStage=1 then id end ) from ProviderReqPackageNegationFileDetails where ProviderReqID=@ProvReqID and deleted=0");
                    vDBHelper.mAddParameter("@ProvReqID", SqlDbType.BigInt, ParameterDirection.Input, ProvReqID);
                    DataTable dtFileHistory = vDBHelper.ExecuteDT();
                    ds.Tables.Add(dtFileHistory);
                }

                vDBHelper.mCreateCommand(CommandType.Text, "Select COUNT(ProviderReqID) AS BankDetailsCount From ProviderReqBankDetails Where ProviderReqID = @ProvReqID And Deleted = 0;");
                vDBHelper.mAddParameter("@ProvReqID", SqlDbType.BigInt, ParameterDirection.Input, ProvReqID);
                DataTable dtBank = vDBHelper.ExecuteDT();
                ds.Tables.Add(dtBank);

                return ds;
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
        }

        public DataSet GetProviderData(string ProviderID)
        {
            try
            {
                DataSet ds = null;

                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "USP_PROVIDER_RETRIEVE");
                vDBHelper.mAddParameter("@ProviderID", SqlDbType.Int, ParameterDirection.Input, ProviderID);
                ds = vDBHelper.Execute();

                DataTable Infrastructure = new DataTable();
                Infrastructure.Columns.Add("ID");
                Infrastructure.Columns.Add("LevelName");
                Infrastructure.Columns.Add("FacilityValue");
                Infrastructure.Columns.Add("DisplayName");
                Infrastructure.Columns.Add("DMSID");
                Infrastructure.Columns.Add("Type");
                Infrastructure.Columns.Add("ddValues");

                DataTable InnerTable = new DataTable();
                InnerTable.Columns.Add("ID");
                InnerTable.Columns.Add("Level3");
                InnerTable.Columns.Add("ProvFacilityID");

                DataTable table1 = ds.Tables[1];
                DataRow flagRow = table1.Rows[0];
                string flag = "", ddvalues = "";
                for (int i = 0; i < table1.Rows.Count - 1; i++)
                {
                    DataRow dr = table1.Rows[i];
                    DataRow nextRow = table1.Rows[i + 1];
                    if (dr["Level1"].ToString() == "Civil and Medical Infrastructure-General" && dr["Level2"].ToString() != "")
                    {
                        if (dr["Level2"].ToString() == nextRow["Level2"].ToString())
                        {
                            InnerTable.Rows.Add(dr["ID"], dr["Level3"], dr["ProvFacilityID"]);
                            flag = "true";
                        }
                        else if (flag == "true")
                        {
                            InnerTable.Rows.Add(dr["ID"], dr["Level3"], dr["ProvFacilityID"]);
                            flag = "false";
                            ddvalues = Newtonsoft.Json.JsonConvert.SerializeObject(InnerTable);
                            InnerTable.Clear();
                            Infrastructure.Rows.Add(dr["ParentID"], dr["Level2"], dr["FacilityValue"], dr["DisplayName"], dr["DMSID"], "DropDown", ddvalues);
                        }
                        else
                        {
                            InnerTable.Clear();
                            Infrastructure.Rows.Add(dr["ID"], dr["Level2"], dr["FacilityValue"], dr["DisplayName"], dr["DMSID"], "TextBox");
                        }
                    }
                }
                ds.Tables.Add(Infrastructure);
                return ds;
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
        }
        #endregion

        #region For Verification Dropdowns
        public DataTable GetVerificationValues()
        {
            try
            {
                vDBHelper.mCreateCommand(CommandType.Text, "select id,Name from Mst_PropertyValues where propertyid=48 and Deleted=0");
                DataTable dt = vDBHelper.ExecuteDT();
                return dt;
            }
            catch (Exception Ex)
            {
                string exMessage = Ex.Message;
                throw;
            }
        }
        #endregion

        #region PostVerification
        public DataSet PostVerification(Verification verf, out string strReturn)
        {
            try
            {
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "Usp_PR_ReqVerification");
                vDBHelper.mAddParameter("@ProviderReqID", SqlDbType.BigInt, ParameterDirection.Input, verf.ProviderReqID);
                vDBHelper.mAddParameter("@statusid", SqlDbType.Int, ParameterDirection.Input, verf.statusid);
                vDBHelper.mAddParameter("@CreatedUserRegionID", SqlDbType.Int, ParameterDirection.Input, verf.CreatedUserRegionID);
                vDBHelper.mAddParameter("@Remarks", SqlDbType.VarChar, ParameterDirection.Input, verf.Remarks);
                vDBHelper.mAddParameter("@ManuallyVerified", SqlDbType.Bit, ParameterDirection.Input, verf.ManuallyVerified);
                vDBHelper.mAddParameter("@IsFHplFormate", SqlDbType.Bit, ParameterDirection.Input, verf.IsFHplFormate);
                vDBHelper.mAddParameter("@ManualVerificationRemarks", SqlDbType.VarChar, ParameterDirection.Input, verf.ManualVerificationRemarks);
                if (verf.RejectionID != 0)
                    vDBHelper.mAddParameter("@RejectionID", SqlDbType.Int, ParameterDirection.Input, verf.RejectionID);
                vDBHelper.mAddParameter("@Msg", SqlDbType.VarChar, 1000, ParameterDirection.Output, "");

                DataSet ds = vDBHelper.ExecutewithSingleparam(out strReturn, "@Msg");
                return ds;
            }
            catch (Exception Ex)
            {
                string exMsg = Ex.Message;
                throw;
            }
        }

        //public void CommunicationTransactionInsert(DataTable Communication, long? ClaimID, int? Slno, int ClaimStageID, DateTime SentDate, int SentUserRegionID,
        // long? IntimationID, bool isCustom = false, bool IsProvider = false)
        //{
        //    try
        //    {
        //        vDBHelper.mCreateCommand(CommandType.StoredProcedure, "USP_Communication_Insert");
        //        vDBHelper.mAddParameter("@Communication", SqlDbType.Structured, ParameterDirection.Input, Communication);
        //        vDBHelper.mAddParameter("@ClaimID", SqlDbType.BigInt, ParameterDirection.Input, ClaimID);
        //        vDBHelper.mAddParameter("@Slno", SqlDbType.Int, ParameterDirection.Input, Slno);
        //        vDBHelper.mAddParameter("@ClaimStageID", SqlDbType.Int, ParameterDirection.Input, ClaimStageID);
        //        vDBHelper.mAddParameter("@SentDate", SqlDbType.DateTime, ParameterDirection.Input, SentDate);
        //        vDBHelper.mAddParameter("@SentUserRegionID", SqlDbType.Int, ParameterDirection.Input, SentUserRegionID);
        //        vDBHelper.mAddParameter("@isCustom", SqlDbType.BigInt, ParameterDirection.Input, isCustom);
        //        vDBHelper.mAddParameter("@IntimationID", SqlDbType.BigInt, ParameterDirection.Input, IntimationID);
        //        vDBHelper.mAddParameter("@CommID", SqlDbType.BigInt, ParameterDirection.Input, -1);
        //        vDBHelper.mAddParameter("@EntityLevel_P6", SqlDbType.Int, ParameterDirection.Input, 296);
        //        vDBHelper.ExecuteNonQuery();
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        public void ProviderCommunicationTransaction_Insert(DataTable Communication, long? ClaimID, int? Slno, int ClaimStageID, DateTime SentDate, int SentUserRegionID,
           long? IntimationID, long ProviderReqID, int? EntityLevel_P6, bool isCustom = false, bool IsProvider = false)
        {
            try
            {
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "USP_Communication_Provider_Insert");
                vDBHelper.mAddParameter("@Communication", SqlDbType.Structured, ParameterDirection.Input, Communication);
                vDBHelper.mAddParameter("@ClaimID", SqlDbType.BigInt, ParameterDirection.Input, ClaimID);
                vDBHelper.mAddParameter("@Slno", SqlDbType.Int, ParameterDirection.Input, Slno);
                vDBHelper.mAddParameter("@ClaimStageID", SqlDbType.Int, ParameterDirection.Input, ClaimStageID);
                vDBHelper.mAddParameter("@SentDate", SqlDbType.DateTime, ParameterDirection.Input, SentDate);
                vDBHelper.mAddParameter("@SentUserRegionID", SqlDbType.Int, ParameterDirection.Input, SentUserRegionID);
                vDBHelper.mAddParameter("@isCustom", SqlDbType.Bit, ParameterDirection.Input, isCustom);
                vDBHelper.mAddParameter("@IntimationID", SqlDbType.BigInt, ParameterDirection.Input, IntimationID);
                vDBHelper.mAddParameter("@EntityLevel_P6", SqlDbType.Int, ParameterDirection.Input, EntityLevel_P6);
                vDBHelper.mAddParameter("@ProviderReqID", SqlDbType.BigInt, ParameterDirection.Input, ProviderReqID);
                vDBHelper.mAddParameter("@IsProvider", SqlDbType.Bit, ParameterDirection.Input, IsProvider);
                vDBHelper.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                string exMsg = ex.Message;
                throw ex;
            }
        }
        #endregion

        #endregion

        #region Package

        #region For Package page Load
        public DataTable GetPackageCombo(string RoleID)
        {

            try
            {
                DataTable dt = new DataTable();
                if (RoleID == "28")
                {
                    dt.Columns.Add("ID", typeof(int));
                    dt.Columns.Add("Name", typeof(string));
                    dt.Rows.Add(178, "Hospital");
                    dt.Rows.Add(166, "TPA");
                }
                else
                {
                    vDBHelper.mCreateCommand(CommandType.Text, "select ID,Name from Mst_Propertyvalues where PropertyId=44 and deleted=0");

                    dt = vDBHelper.ExecuteDT();

                }
                return dt;
            }
            catch (Exception Ex)
            {
                throw Ex;
            }

        }
        #endregion

        #region For Package form- procedureData
        public DataTable GetFacilities(string ProvReqID, int Format)
        {
            try
            {
                if (Format == 1)
                {
                    vDBHelper.mCreateCommand(CommandType.Text, "select PR.FacilityID,isnull(case when Displayname<>'' then displayname end,Level2) Name,PR.Percentage from ProviderReqFacilities PR Inner Join Mst_Facility F on F.ID=PR.FacilityID " +
                        "where ParentID=1 and Level1='Bed Strength' and ProviderReqID=@ProvReqID and PR.Deleted=0 Order by facilityID");
                    vDBHelper.mAddParameter("@ProvReqID", SqlDbType.BigInt, ParameterDirection.Input, ProvReqID);
                    DataTable dt = vDBHelper.ExecuteDT();
                    return dt;
                }
                else
                {
                    vDBHelper.mCreateCommand(CommandType.Text, "select F.ID as FacilityID,Level2 Name,0 as Percentage from  Mst_Facility F " +
                        " where ParentID=1 and  Level1='Bed Strength'  and Level2!='Others' Order by facilityID");
                    vDBHelper.mAddParameter("@ProvReqID", SqlDbType.BigInt, ParameterDirection.Input, ProvReqID);
                    DataTable dt = vDBHelper.ExecuteDT();
                    return dt;

                }
            }
            catch (Exception Ex)
            {
                throw;
            }

        }

        public DataSet GetProcedures(string ProvReqID, string MOUType, string MouEntityID, int Formate)
        {
            try
            {
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "USP_LoadPackageData");
                vDBHelper.mAddParameter("@ProvReqID", SqlDbType.BigInt, ParameterDirection.Input, ProvReqID);
                vDBHelper.mAddParameter("@MOUType", SqlDbType.Int, ParameterDirection.Input, MOUType);
                vDBHelper.mAddParameter("@MouEntityID", SqlDbType.Int, ParameterDirection.Input, MouEntityID);
                vDBHelper.mAddParameter("@Formate", SqlDbType.Int, ParameterDirection.Input, Formate);
                DataSet ds = vDBHelper.Execute();


                //return Newtonsoft.Json.JsonConvert.SerializeObject(ds);
                DataTable NewTable = new DataTable();
                NewTable.Columns.Add("TPAProcID", typeof(int));
                NewTable.Columns.Add("Discount", typeof(int));
                NewTable.Columns.Add("LOS");
                NewTable.Columns.Add("EffectiveDate");
                NewTable.Columns.Add("Stage");
                NewTable.Columns.Add("Inclusions");
                NewTable.Columns.Add("Exclusions");
                NewTable.Columns.Add("facValues");

                DataTable InnerTable = new DataTable();
                InnerTable.Columns.Add("FacilityID");
                InnerTable.Columns.Add("Amount", typeof(int));

                DataTable dt = new DataTable();
                dt = ds.Tables[2];

                dt.DefaultView.Sort = "EffectiveDate";
                DataTable dt1 = dt.DefaultView.ToTable();//"TPAProcID";
                bool newrow = false;
                string facValues = "";

                for (int i = 0; i < dt1.Rows.Count; i++)
                {
                    DataRow dr = dt1.Rows[i];
                    DataRow nextRow = dt1.Rows[i];
                    if (i != dt1.Rows.Count - 1)
                    {
                        nextRow = dt1.Rows[i + 1];
                    }
                    else
                    {
                        if (InnerTable.Rows.Count > 0)
                        {
                            DataRow d = InnerTable.Rows[InnerTable.Rows.Count - 1];
                            if (dr["FacilityID"].ToString() != d["FacilityID"].ToString())
                                InnerTable.Rows.Add(dr["FacilityID"], Convert.ToInt32(dr["Amount"]));
                        }
                        else
                            InnerTable.Rows.Add(dr["FacilityID"], dr["Amount"]);
                        facValues = Newtonsoft.Json.JsonConvert.SerializeObject(InnerTable);
                        NewTable.Rows.Add(dr["TPAProcID"], dr["Discount"], dr["LOS"], dr["EffectiveDate"], dr["Stage"], dr["Inclusions"], dr["Exclusions"], facValues);
                    }
                    if (dr["TPAProcID"].ToString() == nextRow["TPAProcID"].ToString())
                    {
                        if (dr["EffectiveDate"].ToString() == nextRow["EffectiveDate"].ToString())
                        {
                            if (InnerTable.Rows.Count > 0)
                            {
                                DataRow d = InnerTable.Rows[InnerTable.Rows.Count - 1];
                                if (dr["FacilityID"].ToString() != d["FacilityID"].ToString())
                                    InnerTable.Rows.Add(dr["FacilityID"], dr["Amount"]);
                            }
                            else
                                InnerTable.Rows.Add(dr["FacilityID"], dr["Amount"]);
                        }
                        else
                        {
                            if (InnerTable.Rows.Count > 0)
                            {
                                DataRow d = InnerTable.Rows[InnerTable.Rows.Count - 1];
                                if (dr["FacilityID"].ToString() != d["FacilityID"].ToString())
                                    InnerTable.Rows.Add(dr["FacilityID"], dr["Amount"]);
                            }
                            else
                                InnerTable.Rows.Add(dr["FacilityID"], dr["Amount"]);
                            facValues = Newtonsoft.Json.JsonConvert.SerializeObject(InnerTable);
                            InnerTable.Clear();
                            NewTable.Rows.Add(dr["TPAProcID"], dr["Discount"], dr["LOS"], dr["EffectiveDate"], dr["Stage"], dr["Inclusions"], dr["Exclusions"], facValues);
                        }
                    }
                    else
                    {

                        if (InnerTable.Rows.Count > 0)
                        {
                            DataRow d = InnerTable.Rows[InnerTable.Rows.Count - 1];
                            if (dr["FacilityID"].ToString() != d["FacilityID"].ToString())
                                InnerTable.Rows.Add(dr["FacilityID"], dr["Amount"]);
                        }
                        else
                            InnerTable.Rows.Add(dr["FacilityID"], dr["Amount"]);
                        facValues = Newtonsoft.Json.JsonConvert.SerializeObject(InnerTable);
                        InnerTable.Clear();
                        NewTable.Rows.Add(dr["TPAProcID"], dr["Discount"], dr["LOS"], dr["EffectiveDate"], dr["Stage"], dr["Inclusions"], dr["Exclusions"], facValues);
                        // facValues = "";
                    }
                }
                //ds.Tables.Remove(ds.Tables[2]);
                ds.Tables.Add(NewTable);


                return ds;
            }
            catch (Exception Ex)
            {
                throw;
            }

        }

        public DataSet GetServices(string ProvReqID, string MOUType, string MouEntityID, int Formate)
        {
            try
            {
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "[USP_LoadServiceData]");
                vDBHelper.mAddParameter("@ProvReqID", SqlDbType.BigInt, ParameterDirection.Input, ProvReqID);
                vDBHelper.mAddParameter("@MOUType", SqlDbType.Int, ParameterDirection.Input, MOUType);
                vDBHelper.mAddParameter("@MouEntityID", SqlDbType.Int, ParameterDirection.Input, MouEntityID);
                vDBHelper.mAddParameter("@Formate", SqlDbType.Int, ParameterDirection.Input, Formate);
                DataSet ds = vDBHelper.Execute();


                //return Newtonsoft.Json.JsonConvert.SerializeObject(ds);
                DataTable NewTable = new DataTable();
                NewTable.Columns.Add("Serviceid", typeof(int));
                NewTable.Columns.Add("Discount", typeof(int));
                NewTable.Columns.Add("UnitType", typeof(int));

                NewTable.Columns.Add("EffectiveDate");
                NewTable.Columns.Add("Stage");
                NewTable.Columns.Add("Inclusions");
                NewTable.Columns.Add("Exclusions");
                NewTable.Columns.Add("IPDisscount");
                NewTable.Columns.Add("OPDisscount");
                NewTable.Columns.Add("facValues");



                DataTable InnerTable = new DataTable();
                InnerTable.Columns.Add("FacilityID");
                InnerTable.Columns.Add("Amount", typeof(int));


                DataTable dt = new DataTable();
                dt = ds.Tables[2];

                dt.DefaultView.Sort = "EffectiveDate";
                DataTable dt1 = dt.DefaultView.ToTable();//"TPAProcID";
                bool newrow = false;
                string facValues = "";

                for (int i = 0; i < dt1.Rows.Count; i++)
                {
                    DataRow dr = dt1.Rows[i];
                    DataRow nextRow = dt1.Rows[i];
                    if (i != dt1.Rows.Count - 1)
                    {
                        nextRow = dt1.Rows[i + 1];
                    }
                    else
                    {
                        if (InnerTable.Rows.Count > 0)
                        {
                            DataRow d = InnerTable.Rows[InnerTable.Rows.Count - 1];
                            if (dr["FacilityID"].ToString() != d["FacilityID"].ToString())
                                InnerTable.Rows.Add(dr["FacilityID"], Convert.ToInt32(dr["Amount"]));
                        }
                        else
                            InnerTable.Rows.Add(dr["FacilityID"], dr["Amount"]);
                        facValues = Newtonsoft.Json.JsonConvert.SerializeObject(InnerTable);
                        NewTable.Rows.Add(dr["Serviceid"], dr["Discount"], dr["UnitType"], dr["EffectiveDate"], dr["Stage"], dr["Inclusions"], dr["Exclusions"], dr["IPDisscount"], dr["OPDisscount"], facValues);
                    }
                    if (dr["Serviceid"].ToString() == nextRow["Serviceid"].ToString())
                    {
                        if (dr["EffectiveDate"].ToString() == nextRow["EffectiveDate"].ToString())
                        {
                            if (InnerTable.Rows.Count > 0)
                            {
                                DataRow d = InnerTable.Rows[InnerTable.Rows.Count - 1];
                                if (dr["FacilityID"].ToString() != d["FacilityID"].ToString())
                                    InnerTable.Rows.Add(dr["FacilityID"], dr["Amount"]);
                            }
                            else
                                InnerTable.Rows.Add(dr["FacilityID"], dr["Amount"]);
                        }
                        else
                        {
                            if (InnerTable.Rows.Count > 0)
                            {
                                DataRow d = InnerTable.Rows[InnerTable.Rows.Count - 1];
                                if (dr["FacilityID"].ToString() != d["FacilityID"].ToString())
                                    InnerTable.Rows.Add(dr["FacilityID"], dr["Amount"]);
                            }
                            else
                                InnerTable.Rows.Add(dr["FacilityID"], dr["Amount"]);
                            facValues = Newtonsoft.Json.JsonConvert.SerializeObject(InnerTable);
                            InnerTable.Clear();
                            NewTable.Rows.Add(dr["Serviceid"], dr["Discount"], dr["UnitType"], dr["EffectiveDate"], dr["Stage"], dr["Inclusions"], dr["Exclusions"], dr["IPDisscount"], dr["OPDisscount"], facValues);
                        }
                    }
                    else
                    {

                        if (InnerTable.Rows.Count > 0)
                        {
                            DataRow d = InnerTable.Rows[InnerTable.Rows.Count - 1];
                            if (dr["FacilityID"].ToString() != d["FacilityID"].ToString())
                                InnerTable.Rows.Add(dr["FacilityID"], dr["Amount"]);
                        }
                        else
                            InnerTable.Rows.Add(dr["FacilityID"], dr["Amount"]);
                        facValues = Newtonsoft.Json.JsonConvert.SerializeObject(InnerTable);
                        InnerTable.Clear();
                        NewTable.Rows.Add(dr["Serviceid"], dr["Discount"], dr["UnitType"], dr["EffectiveDate"], dr["Stage"], dr["Inclusions"], dr["Exclusions"], dr["IPDisscount"], dr["OPDisscount"], facValues);
                        // facValues = "";
                    }
                }
                //ds.Tables.Remove(ds.Tables[2]);
                ds.Tables.Add(NewTable);


                return ds;
            }
            catch (Exception Ex)
            {
                throw;
            }

        }

        public DataTable GetUploadPackageHistory(long ProviderID, long MOUID, int type)
        {
            try
            {
                DataTable dt = new DataTable();
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "USP_LoadProviderPackageHistory");
                vDBHelper.mAddParameter("@ProviderID", SqlDbType.BigInt, ParameterDirection.Input, ProviderID);
                vDBHelper.mAddParameter("@MOUID", SqlDbType.VarChar, ParameterDirection.Input, MOUID);
                vDBHelper.mAddParameter("@Type", SqlDbType.Int, ParameterDirection.Input, type);
                dt = vDBHelper.ExecuteDT();
                return dt;
            }
            catch (Exception Ex)
            {
                throw;
            }
        }

        public DataSet GetProviderPackageData(string ProviderID, string MOUID)
        {
            try
            {
                vDBHelper.mCreateCommand(CommandType.Text, "select isnull(PackagePercentage,0) as PackagePercentage from ProviderMOU where ProviderID=@ProviderID and ID=@MOUID and deleted=0 ;Select * from ProviderPackage where ProviderID=@ProviderID and  MOUID=@MOUID and deleted=0");
                //vDBHelper.mCreateCommand(CommandType.Text, "Select * from ProviderPackage where ProviderID=@ProviderID and  MOUID=@MOUID and deleted=0");
                vDBHelper.mAddParameter("@ProviderID", SqlDbType.BigInt, ParameterDirection.Input, ProviderID);
                vDBHelper.mAddParameter("@MOUID", SqlDbType.Int, ParameterDirection.Input, MOUID);
                DataSet ds = vDBHelper.Execute();
                return ds;
            }
            catch (Exception Ex)
            {
                throw;
            }
        }

        public DataTable GetEffectiveDateLoad(string MOUID)
        {
            try
            {
                DataTable dt = new DataTable();

                string Query = "select IPPercentage,CONVERT(VARCHAR(11), StartDate, 105) as EffectiveDate from providerMOU where ID = @MOUID";
                vDBHelper.mCreateCommand(CommandType.Text, Query);
                // vDBHelper.mAddParameter("@ProviderID", SqlDbType.BigInt, ParameterDirection.Input, ProviderID);
                vDBHelper.mAddParameter("@MOUID", SqlDbType.VarChar, ParameterDirection.Input, MOUID);
                dt = vDBHelper.ExecuteDT();
                return dt;
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
        }
        public DataSet GetProviderServiceData(string ProviderID, string MOUID, DateTime? EfDate)
        {
            try
            {
                //vDBHelper.mCreateCommand(CommandType.Text, "  select isnull(IPPercentage,0) as IPPercentage,isnull(OPPercentage,0) as OPPercentage from ProviderMOU where ProviderID=@ProviderID and ID=@MOUID and deleted=0 ;Select * from ProviderTariff where ProviderID=@ProviderID and  MOUID=@MOUID and deleted=0");
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "USP_ProviderServiceData");
                vDBHelper.mAddParameter("@ProviderID", SqlDbType.BigInt, ParameterDirection.Input, ProviderID);
                vDBHelper.mAddParameter("@MOUID", SqlDbType.Int, ParameterDirection.Input, MOUID);
                vDBHelper.mAddParameter("@EffectiveDate", SqlDbType.DateTime, ParameterDirection.Input, EfDate == null ? (object)DBNull.Value : EfDate);
                DataSet ds = vDBHelper.Execute();
                return ds;
            }
            catch (Exception Ex)
            {
                throw;
            }
        }
        public DataTable GetMst_Facilities()
        {
            try
            {
                DataTable dt = new DataTable();
                vDBHelper.mCreateCommand(CommandType.Text, " select F.ID as FacilityID,Level2 Name,0 as Percentage from  Mst_Facility F where ParentID=1 and  Level1='Bed Strength'  and Level2!='Others' Order by facilityID");
                dt = vDBHelper.ExecuteDT();
                return dt;
            }
            catch (Exception Ex)
            {
                string errorMsg = Ex.Message;
                throw;
            }
        }

        public DataTable GetProvider_Facilities(long ProviderID)
        {
            try
            {
                DataTable dt = new DataTable();
                vDBHelper.mCreateCommand(CommandType.Text, " select F.ID as FacilityID,Level2 Name,isnull(pf.Percentage,0) as Percentage from  Mst_Facility F" +
                    " left join ProviderFacilities pf on pf.ProviderID=@ProviderID and pf.Deleted=0 and f.ID=pf.FacilityID " +
                    "  where ParentID=1 and  Level1='Bed Strength'  and Level2!='Others' Order by facilityID");
                vDBHelper.mAddParameter("@ProviderID", SqlDbType.BigInt, ParameterDirection.Input, ProviderID);
                dt = vDBHelper.ExecuteDT();
                return dt;
            }
            catch (Exception Ex)
            {
                throw;
            }
        }

        public DataTable GetMst_Services()
        {
            try
            {
                DataTable dt = new DataTable();
                vDBHelper.mCreateCommand(CommandType.Text, "select s.id as ServiceID,s.SERVICECODE as Category,s.Name,ps.Name as ParentName,isnull (pv.Name,'') as UnitType   from Mst_Services s " +
                    "inner join  Mst_Services ps on ps.ID=s.ParentID left join Mst_PropertyValues pv on pv.ID=s.UnitType_P67 where s.deleted=0 and s.ParentID!=0 and s.IProviderServices=1   order by s.ID ");
                dt = vDBHelper.ExecuteDT();
                return dt;
            }
            catch (Exception Ex)
            {
                throw;
            }
        }

        public DataTable GetProviderMst_Services()
        {
            try
            {
                DataTable dt = new DataTable();
                vDBHelper.mCreateCommand(CommandType.Text, "select s.id as ServiceID,s.SERVICECODE as Category,s.Name,ps.Name as ParentName,isnull (pv.Name,'') as UnitType   from Mst_Services s " +
                    "inner join  Mst_Services ps on ps.ID=s.ParentID left join Mst_PropertyValues pv on pv.ID=s.UnitType_P67 where s.deleted=0 and s.ParentID!=0 and s.IProviderServices=1 and s.id not in(2,3) order by s.ID ");
                dt = vDBHelper.ExecuteDT();
                return dt;
            }
            catch (Exception Ex)
            {
                throw;
            }
        }

        public DataTable GetMst_Procedures()
        {
            try
            {
                DataTable dt = new DataTable();
                vDBHelper.mCreateCommand(CommandType.Text, "select id TPAProcID,Code as Category,Level3 Name,Level1 as SPECIALITY from TPAProcedures where deleted=0 and code>0  and level3 is not null and level3<>'' order by ID    ");
                dt = vDBHelper.ExecuteDT();
                return dt;
            }
            catch (Exception Ex)
            {
                throw;
            }
        }

        public DataTable GetNegotiationHistory(string ProvReqID, string MOUType, string MouEntityID, int type)
        {
            try
            {
                vDBHelper.mCreateCommand(CommandType.Text, "select ID,FileURL,ProviderReqID,CreatedUserRegionID,CreatedDateTime,ModifiedUserRegionID,ModifiedDateTime,IsLatest=case when IsLatest=0 then 'No' else 'Yes' end," +
    "IsFinal=case when IsFinal=0 then 'No' else 'Yes' end	,FileFormat=case when FileFormat=0 then 'Not FHPL Format' else 'FHPL Format' end ,FileName,Deleted,UploadedBy " +
    " from ProviderReqPackageNegationFileDetails where ProviderReqID=@ProvReqID and deleted=0 and FileStage=@type");
                vDBHelper.mAddParameter("@ProvReqID", SqlDbType.BigInt, ParameterDirection.Input, ProvReqID);
                vDBHelper.mAddParameter("@MOUType", SqlDbType.Int, ParameterDirection.Input, MOUType);
                vDBHelper.mAddParameter("@MouEntityID", SqlDbType.Int, ParameterDirection.Input, MouEntityID);
                vDBHelper.mAddParameter("@type", SqlDbType.Int, ParameterDirection.Input, type);
                DataTable dt = vDBHelper.ExecuteDT();
                return dt;
            }
            catch (Exception Ex)
            {
                throw;
            }
        }

        public DataTable GetPackageFilePathDeatils(long FileID)
        {
            try
            {
                DataTable dt = new DataTable();
                vDBHelper.mCreateCommand(CommandType.Text, "select * from ProviderReqPackageNegationFileDetails where ID=@FileID");
                vDBHelper.mAddParameter("@FileID", SqlDbType.BigInt, ParameterDirection.Input, FileID);
                dt = vDBHelper.ExecuteDT();
                return dt;
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
        }
        public DataTable GetProviderFileDetailsForApproval(long ProviderID)
        {
            try
            {
                DataTable dt = new DataTable();
                //get details of filename
                vDBHelper.mCreateCommand(CommandType.Text, "select IsVerifiedFileName from Provider where ID=@ProviderID");
                vDBHelper.mAddParameter("@ProviderID", SqlDbType.BigInt, ParameterDirection.Input, ProviderID);
                dt = vDBHelper.ExecuteDT();
                return dt;
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
        }

        public DataTable GetDownloadFlaggingFile(long ProviderID, int RelevantID, int ProviderStatusID)
        {
            try
            {
                DataTable dt = new DataTable();
                //get details of filename
                vDBHelper.mCreateCommand(CommandType.Text, @"SELECT AuthorizationFileUrl FROM ProviderCategory WHERE ProviderID = @ProviderID AND RelevantID = @RelevantID AND ProviderStatusID = @ProviderStatusID AND Deleted = 0");
                vDBHelper.mAddParameter("@ProviderID", SqlDbType.BigInt, ParameterDirection.Input, ProviderID);
                vDBHelper.mAddParameter("@RelevantID", SqlDbType.Int, ParameterDirection.Input, RelevantID);
                vDBHelper.mAddParameter("@ProviderStatusID", SqlDbType.Int, ParameterDirection.Input, ProviderStatusID);
                dt = vDBHelper.ExecuteDT();
                return dt;
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
        }
        public DataTable GetProvideDMSFilePathDeatils(long DMSID)
        {
            try
            {
                DataTable dt = new DataTable();
                //vDBHelper.mCreateCommand(CommandType.Text, "select *,FilePath+SystemFileName as FileURL from DMSFileinfo_Provider where ID=@DMSID");
                //Unable to view uploaded documents-Provider (SP-1442)
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "USP_GetProviderDeclarationFileInfo");
                //End of Unable to view uploaded documents-Provider (SP-1442) 
                vDBHelper.mAddParameter("@DMSID", SqlDbType.BigInt, ParameterDirection.Input, DMSID);
                dt = vDBHelper.ExecuteDT();
                return dt;
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
        }
        #endregion

        #region For CreatePackage
        public string CreatePackage(Package package)
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("TPAProcID", typeof(int));
                dt.Columns.Add("Inclusions", typeof(string));
                dt.Columns.Add("Exclusions", typeof(string));
                dt.Columns.Add("FacilityID", typeof(int));
                dt.Columns.Add("Amount", typeof(decimal));
                dt.Columns.Add("Discount", typeof(decimal));
                dt.Columns.Add("LOS", typeof(int));
                dt.Columns.Add("EffectiveDate", typeof(string));

                foreach (Packageproccedures pp in package.Packages)
                {
                    dt.Rows.Add(pp.TPAProcID, pp.Inclusions, pp.Exclusions, pp.FacilityID, pp.Amount, pp.Discount, pp.LOS, null);
                }
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "USP_ProReqpackage_Insert");
                vDBHelper.mAddParameter("@ProvReqID", SqlDbType.BigInt, ParameterDirection.Input, package.ProvReqID);
                vDBHelper.mAddParameter("@MOUTypeID", SqlDbType.Int, ParameterDirection.Input, package.MOUTypeID);
                vDBHelper.mAddParameter("@MOUEntityID", SqlDbType.Int, ParameterDirection.Input, package.MOUEntityID);
                vDBHelper.mAddParameter("@CreatedUserRegionID", SqlDbType.Int, ParameterDirection.Input, package.CreatedUserRegionID);
                vDBHelper.mAddParameter("@ProvReqPackage", SqlDbType.Structured, ParameterDirection.Input, dt);
                vDBHelper.mAddParameter("@Msg", SqlDbType.VarChar, 1000, ParameterDirection.Output, "");
                string strReturn;
                vDBHelper.ExecuteNonQuery(out strReturn, "@Msg");
                return strReturn;
            }
            catch (Exception Ex)
            {
                // return "Failed" + Ex.Message;
                throw;
            }

        }

        public string UploadPackage(Package package, string FileURL, string FileName, int FileFormat, int IsProvider, int Type)
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("TPAProcID", typeof(int));
                dt.Columns.Add("Inclusions", typeof(string));
                dt.Columns.Add("Exclusions", typeof(string));
                dt.Columns.Add("FacilityID", typeof(int));
                dt.Columns.Add("Amount", typeof(decimal));
                dt.Columns.Add("Discount", typeof(decimal));
                dt.Columns.Add("LOS", typeof(int));
                dt.Columns.Add("EffectiveDate", typeof(string));

                if (FileFormat != 0)
                {

                    foreach (Packageproccedures pp in package.Packages)
                    {
                        string efdate = null;

                        Byte los1 = 0;
                        if (Byte.TryParse(pp.LOS.ToString(), out los1))
                            los1 = Convert.ToByte(pp.LOS);

                        if (pp.EffectiveDate != null)
                            efdate = pp.EffectiveDate.ToString();
                        dt.Rows.Add(pp.TPAProcID, pp.Inclusions, pp.Exclusions, pp.FacilityID, pp.Amount, pp.Discount, los1, System.DateTime.Now.ToString("yyyy-MM-dd"));
                    }
                }
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "USP_ProReqpackage_Upload");
                vDBHelper.mAddParameter("@ProvReqID", SqlDbType.BigInt, ParameterDirection.Input, package.ProvReqID);
                vDBHelper.mAddParameter("@MOUTypeID", SqlDbType.Int, ParameterDirection.Input, package.MOUTypeID);
                vDBHelper.mAddParameter("@MOUEntityID", SqlDbType.Int, ParameterDirection.Input, package.MOUEntityID);
                vDBHelper.mAddParameter("@CreatedUserRegionID", SqlDbType.Int, ParameterDirection.Input, package.CreatedUserRegionID);
                vDBHelper.mAddParameter("@ProvReqPackage", SqlDbType.Structured, ParameterDirection.Input, dt);
                vDBHelper.mAddParameter("@FileURL", SqlDbType.VarChar, ParameterDirection.Input, FileURL);
                vDBHelper.mAddParameter("@FileName", SqlDbType.VarChar, ParameterDirection.Input, FileName);
                vDBHelper.mAddParameter("@FileFormat", SqlDbType.Int, ParameterDirection.Input, FileFormat);
                vDBHelper.mAddParameter("@UploadedBy", SqlDbType.Int, ParameterDirection.Input, IsProvider);
                vDBHelper.mAddParameter("@Type", SqlDbType.Int, ParameterDirection.Input, Type);
                vDBHelper.mAddParameter("@Msg", SqlDbType.VarChar, 1000, ParameterDirection.Output, "");
                string strReturn;
                vDBHelper.ExecuteNonQuery(out strReturn, "@Msg");
                return strReturn;
            }
            catch (Exception Ex)
            {
                // return "Failed" + Ex.Message;
                throw;
            }

        }

        public string UploadPackage_NetworkUser(List<Packageproccedures> package, string FileURL, string FileName, long ProviderID, string MOUID, int FileFormat, int CreatedUserRegionID)
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("TPAProcID", typeof(int));
                dt.Columns.Add("Inclusions", typeof(string));
                dt.Columns.Add("Exclusions", typeof(string));
                dt.Columns.Add("FacilityID", typeof(int));
                dt.Columns.Add("Amount", typeof(decimal));
                dt.Columns.Add("Discount", typeof(decimal));
                dt.Columns.Add("LOS", typeof(int));
                dt.Columns.Add("EffectiveDate", typeof(string));

                foreach (Packageproccedures pp in package)
                {
                    string efdate = null;

                    Byte los1 = 0;
                    if (Byte.TryParse(pp.LOS.ToString(), out los1))
                        los1 = Convert.ToByte(pp.LOS);

                    if (pp.EffectiveDate != null)
                        efdate = pp.EffectiveDate.ToString();
                    dt.Rows.Add(pp.TPAProcID, pp.Inclusions, pp.Exclusions, pp.FacilityID, pp.Amount, pp.Discount, los1, System.DateTime.Now.ToString("yyyy-MM-dd"));
                }

                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "USP_ProviderPackage_Upload");
                vDBHelper.mAddParameter("@ProviderID", SqlDbType.BigInt, ParameterDirection.Input, ProviderID);
                vDBHelper.mAddParameter("@MOUID", SqlDbType.VarChar, ParameterDirection.Input, MOUID);
                vDBHelper.mAddParameter("@CreatedUserRegionID", SqlDbType.Int, ParameterDirection.Input, CreatedUserRegionID);
                vDBHelper.mAddParameter("@ProviderPackage", SqlDbType.Structured, ParameterDirection.Input, dt);
                vDBHelper.mAddParameter("@FileURL", SqlDbType.VarChar, ParameterDirection.Input, FileURL);
                vDBHelper.mAddParameter("@FileName", SqlDbType.VarChar, ParameterDirection.Input, FileName);
                vDBHelper.mAddParameter("@FileFormat", SqlDbType.VarChar, ParameterDirection.Input, FileFormat);
                vDBHelper.mAddParameter("@Msg", SqlDbType.VarChar, 1000, ParameterDirection.Output, "");
                string strReturn;
                vDBHelper.ExecuteNonQuery(out strReturn, "@Msg");

                return strReturn;
            }
            catch (Exception Ex)
            {
                // return "Failed" + Ex.Message;
                throw;
            }

        }

        public string UploadPackage_OtherFiles_NetworkUser(string FileURL, string FileName, long ProviderID, string MOUID, int FileFormat, int CreatedUserRegionID)
        {
            try
            {
                string packageFileQuery = "INSERT INTO ProviderReqPackageNegationFileDetails(FileURL, ProviderReqID, ProviderID, CreatedUserRegionID, CreatedDateTime, FileFormat, [FileName], IsLatest, IsFinal, Deleted, UploadedBy, FileStage, MOUIDs)" +
                                          " VALUES(@FileURL, 0, @ProviderID, @CreatedUserRegionID, GETDATE(), @FileFormat, @FileName, 1, 0, 0, @UploadedBy, 0, @MOUID); ";
                vDBHelper.mCreateCommand(CommandType.Text, packageFileQuery);
                vDBHelper.mAddParameter("@ProviderID", SqlDbType.BigInt, ParameterDirection.Input, ProviderID);
                vDBHelper.mAddParameter("@MOUID", SqlDbType.VarChar, ParameterDirection.Input, MOUID);
                vDBHelper.mAddParameter("@UploadedBy", SqlDbType.Int, ParameterDirection.Input, 0);
                vDBHelper.mAddParameter("@CreatedUserRegionID", SqlDbType.Int, ParameterDirection.Input, CreatedUserRegionID);
                vDBHelper.mAddParameter("@FileURL", SqlDbType.VarChar, ParameterDirection.Input, FileURL);
                vDBHelper.mAddParameter("@FileName", SqlDbType.VarChar, ParameterDirection.Input, FileName);
                vDBHelper.mAddParameter("@FileFormat", SqlDbType.VarChar, ParameterDirection.Input, FileFormat);
                vDBHelper.ExecuteNonQuery();
                return "Package updated";
            }
            catch (Exception Ex)
            {
                return "Failed" + Ex.Message;
            }
        }

        public string FinalUploadPackage(long ProviderReqID, int CreatedUserRegionID)
        {
            try
            {
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "USP_ProReqpackage_FinalUpload");
                vDBHelper.mAddParameter("@ProviderReqID", SqlDbType.BigInt, ParameterDirection.Input, ProviderReqID);
                vDBHelper.mAddParameter("@CreatedUserRegionID", SqlDbType.Int, ParameterDirection.Input, CreatedUserRegionID);
                vDBHelper.mAddParameter("@Msg", SqlDbType.VarChar, 1000, ParameterDirection.Output, "");
                string strReturn;
                vDBHelper.ExecuteNonQuery(out strReturn, "@Msg");
                return strReturn;
            }
            catch (Exception Ex)
            {
                // return "Failed" + Ex.Message;
                throw;
            }
        }

        public DataTable GetProviderCommunication(long ProviderReqID)
        {
            try
            {
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "USP_ProReq_Communication");
                vDBHelper.mAddParameter("@ProviderReqID", SqlDbType.BigInt, ParameterDirection.Input, ProviderReqID);
                DataTable dt = vDBHelper.ExecuteDT();
                return dt;
            }
            catch (Exception Ex)
            {
                // return "Failed" + Ex.Message;
                throw;
            }
        }
        public DataTable CheckServiceDiscountHistory(Int64 ProviderID, string MOUID)
        {
            try
            {
                DataTable dt = new DataTable();
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "Usp_GetServiceDiscountHistory");
                vDBHelper.mAddParameter("@ProviderID", SqlDbType.BigInt, ParameterDirection.Input, ProviderID);
                vDBHelper.mAddParameter("@MOUID", SqlDbType.VarChar, ParameterDirection.Input, MOUID);
                dt = vDBHelper.ExecuteDT();
                return dt;
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
        }
      
        public DataTable GetDistinctEffectiveDates(string MOUID)
        {
            try
            {
                DataTable dt = new DataTable();

                // string Query = "select distinct(CONVERT(VARCHAR(11), EffectiveDate, 105)) as EffectiveDate from providerTariff where MOUID = @MOUID";
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "Usp_GetExclusiveServiceEffectiveDate");
                // vDBHelper.mAddParameter("@ProviderID", SqlDbType.BigInt, ParameterDirection.Input, ProviderID);
                vDBHelper.mAddParameter("@MOUID", SqlDbType.VarChar, ParameterDirection.Input, MOUID);
                dt = vDBHelper.ExecuteDT();
                return dt;
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
        }
        public DataSet InsertandupdateServicewiseTariff(decimal IPDisscount, decimal OPDisscount, string MOUID, string _LocaSrvID, Int64 _providerID, DateTime? EffectiveDate, String _Remarks, int CreatedUserRegionID)
        {
            try

            {
                object effDate;
                string effStr = EffectiveDate?.ToString("dd-MM-yyy");

                if (string.IsNullOrWhiteSpace(effStr))
                {
                    effDate = DateTime.Now.Date;
                }
                else
                {
                    effDate = DateTime.ParseExact(
                        effStr,
                        new[] { "dd-MM-yyyy", "dd/MM/yyyy", "yyyy-MM-dd", "MM/dd/yyyy" },
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None);
                }
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "Usp_ServiceSpecific_ProviderTariff_Update");
                //vDBHelper.mAddParameter("@ServiceSpecificMouTariff", SqlDbType.Structured, ParameterDirection.Input, oPackageDiscountDetails);
                vDBHelper.mAddParameter("@MOUID", SqlDbType.VarChar, ParameterDirection.Input, MOUID);
                vDBHelper.mAddParameter("@ServiceID", SqlDbType.Int, ParameterDirection.Input, _LocaSrvID);
                vDBHelper.mAddParameter("@ProviderID", SqlDbType.BigInt, ParameterDirection.Input, _providerID);
                vDBHelper.mAddParameter("@OPDiscount", SqlDbType.BigInt, ParameterDirection.Input, OPDisscount);
                vDBHelper.mAddParameter("@IPDiscount", SqlDbType.BigInt, ParameterDirection.Input, IPDisscount);
                vDBHelper.mAddParameter("@Remarks", SqlDbType.VarChar, int.MaxValue, ParameterDirection.Input, _Remarks);
                vDBHelper.mAddParameter("@CreatedUserRegionID", SqlDbType.BigInt, ParameterDirection.Input, CreatedUserRegionID);

                vDBHelper.mAddParameter("@EffectiveDate", SqlDbType.DateTime, ParameterDirection.Input, effDate);
                vDBHelper.mAddParameter("@Msg", SqlDbType.VarChar, 1000, ParameterDirection.Output, DBNull.Value);
                DataSet ds = vDBHelper.Execute();
                return ds;
            }
            catch (Exception Ex)
            {
                //return "Failed" + Ex.Message;
                throw;
            }
        }
        public string UploadServices(Service Service, string FileURL, string FileName, int FileFormat, int IsProvider, int Type)
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("ServiceID", typeof(int));
                dt.Columns.Add("FacilityID", typeof(int));
                dt.Columns.Add("Amount", typeof(decimal));
                dt.Columns.Add("Discount", typeof(decimal));
                dt.Columns.Add("OPDiscount", typeof(decimal));
                dt.Columns.Add("UnitType", typeof(int));
                dt.Columns.Add("EffectiveDate", typeof(string));

                if (FileFormat != 0)
                {
                    foreach (PackageServices pp in Service.Services)
                    {
                        string efdate = null;
                        if (pp.EffectiveDate != null)
                            efdate = pp.EffectiveDate.ToString();
                        dt.Rows.Add(pp.ServiceID, pp.FacilityID, pp.Amount, pp.Discount, 0, pp.UnitType, System.DateTime.Now.ToString("yyyy-MM-dd"));
                    }
                }
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "USP_ProReqServices_Upload");
                vDBHelper.mAddParameter("@ProvReqID", SqlDbType.BigInt, ParameterDirection.Input, Service.ProvReqID);
                vDBHelper.mAddParameter("@MOUTypeID", SqlDbType.Int, ParameterDirection.Input, Service.MOUTypeID);
                vDBHelper.mAddParameter("@MOUEntityID", SqlDbType.Int, ParameterDirection.Input, Service.MOUEntityID);
                vDBHelper.mAddParameter("@CreatedUserRegionID", SqlDbType.Int, ParameterDirection.Input, Service.CreatedUserRegionID);
                vDBHelper.mAddParameter("@ProvReqServices", SqlDbType.Structured, ParameterDirection.Input, dt);
                vDBHelper.mAddParameter("@FileURL", SqlDbType.VarChar, ParameterDirection.Input, FileURL);
                vDBHelper.mAddParameter("@FileName", SqlDbType.VarChar, ParameterDirection.Input, FileName);
                vDBHelper.mAddParameter("@FileFormat", SqlDbType.Int, ParameterDirection.Input, FileFormat);
                vDBHelper.mAddParameter("@UploadedBy", SqlDbType.Int, ParameterDirection.Input, IsProvider);
                vDBHelper.mAddParameter("@Type", SqlDbType.Int, ParameterDirection.Input, Type);
                vDBHelper.mAddParameter("@Msg", SqlDbType.VarChar, 1000, ParameterDirection.Output, "");
                string strReturn;
                vDBHelper.ExecuteNonQuery(out strReturn, "@Msg");
                return strReturn;
            }
            catch (Exception Ex)
            {
                // return "Failed" + Ex.Message;
                throw;
            }
        }

        public string UploadServices_NetworkUser(List<PackageServices> Service, string FileURL, string FileName, int FileFormat, long ProviderID, string MOUID, int CreatedUserRegionID, DateTime EffectiveDate, string Remarks)
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("ServiceID", typeof(int));
                dt.Columns.Add("FacilityID", typeof(int));
                dt.Columns.Add("Amount", typeof(decimal));
                dt.Columns.Add("Discount", typeof(decimal));
                dt.Columns.Add("OPDiscount", typeof(decimal));
                dt.Columns.Add("UnitType", typeof(int));
                dt.Columns.Add("EffectiveDate", typeof(string));

                if (FileFormat != 0)
                {
                    foreach (PackageServices pp in Service)
                    {
                        string efdate = null;
                        if (pp.EffectiveDate != null)
                            efdate = pp.EffectiveDate.ToString();
                        dt.Rows.Add(pp.ServiceID, pp.FacilityID, pp.Amount, pp.Discount, pp.OPDiscount, pp.UnitType, pp.EffectiveDate);
                    }
                }
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "USP_ProviderServices_Upload");
                vDBHelper.mAddParameter("@ProviderID", SqlDbType.BigInt, ParameterDirection.Input, ProviderID);
                vDBHelper.mAddParameter("@MOUID", SqlDbType.VarChar, ParameterDirection.Input, MOUID);
                vDBHelper.mAddParameter("@ProviderTariff", SqlDbType.Structured, ParameterDirection.Input, dt);
                vDBHelper.mAddParameter("@FileURL", SqlDbType.VarChar, ParameterDirection.Input, FileURL);
                vDBHelper.mAddParameter("@FileName", SqlDbType.VarChar, ParameterDirection.Input, FileName);
                vDBHelper.mAddParameter("@FileFormat", SqlDbType.Int, ParameterDirection.Input, FileFormat);
                vDBHelper.mAddParameter("@EffectiveDate", SqlDbType.DateTime, ParameterDirection.Input, EffectiveDate);
                vDBHelper.mAddParameter("@Remarks", SqlDbType.VarChar, ParameterDirection.Input, Remarks);
                vDBHelper.mAddParameter("@CreatedUserRegionID", SqlDbType.Int, ParameterDirection.Input, CreatedUserRegionID);
                vDBHelper.mAddParameter("@Msg", SqlDbType.VarChar, 1000, ParameterDirection.Output, "");
                string strReturn;
                vDBHelper.ExecuteNonQuery(out strReturn, "@Msg");
                return strReturn;
            }
            catch (Exception Ex)
            {
                // return "Failed" + Ex.Message;
                throw;
            }
        }
        public string UploadServices_OtherFiles_NetworkUser(string FileURL, string FileName, int FileFormat, long ProviderID, string MOUID, int CreatedUserRegionID)
        {
            try
            {
                vDBHelper.mCreateCommand(CommandType.Text, "INSERT INTO ProviderReqPackageNegationFileDetails(FileURL,ProviderReqID,ProviderID,CreatedUserRegionID,CreatedDateTime,FileFormat,[FileName],IsLatest,IsFinal,Deleted,UploadedBy,FileStage,MOUIDs)" +
                    " VALUES(@FileURL,0,@ProviderID,@CreatedUserRegionID,GETDATE(),@FileFormat,@FileName,1,0,0,@UploadedBy,1,@MOUID)");
                vDBHelper.mAddParameter("@ProviderID", SqlDbType.BigInt, ParameterDirection.Input, ProviderID);
                vDBHelper.mAddParameter("@MOUID", SqlDbType.VarChar, ParameterDirection.Input, MOUID);
                vDBHelper.mAddParameter("@FileURL", SqlDbType.VarChar, ParameterDirection.Input, FileURL);
                vDBHelper.mAddParameter("@FileName", SqlDbType.VarChar, ParameterDirection.Input, FileName);
                vDBHelper.mAddParameter("@FileFormat", SqlDbType.Int, ParameterDirection.Input, FileFormat);
                vDBHelper.mAddParameter("@UploadedBy", SqlDbType.Int, ParameterDirection.Input, CreatedUserRegionID);
                vDBHelper.mAddParameter("@CreatedUserRegionID", SqlDbType.Int, ParameterDirection.Input, CreatedUserRegionID);
                //vDBHelper.mAddParameter("@Msg", SqlDbType.VarChar, 1000, ParameterDirection.Output, "");
                //string strReturn;
                //vDBHelper.ExecuteNonQuery(out strReturn, "@Msg");
                //return strReturn;
                vDBHelper.ExecuteNonQuery();
                return "Tariff rates updated";
            }
            catch (Exception Ex)
            {
                return "Failed" + Ex.Message;
                //throw;
            }
        }

        public DataSet UpdateProviderTariffDetails(List<PackageServices> Service, long ProviderID, long MOUID, long CreatedUserRegionID, out string strReturn)
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("ServiceID", typeof(int));
                dt.Columns.Add("FacilityID", typeof(int));
                dt.Columns.Add("Amount", typeof(decimal));
                dt.Columns.Add("Discount", typeof(decimal));
                dt.Columns.Add("OPDiscount", typeof(decimal));
                dt.Columns.Add("UnitType", typeof(int));
                dt.Columns.Add("EffectiveDate", typeof(string));

                foreach (PackageServices pp in Service)
                {
                    string efdate = null;
                    if (pp.EffectiveDate != null)
                        efdate = pp.EffectiveDate.ToString();
                    dt.Rows.Add(pp.ServiceID, pp.FacilityID, pp.Amount, pp.Discount, pp.OPDiscount, pp.UnitType, System.DateTime.Now.ToString("yyyy-MM-dd"));
                }

                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "[USP_ProviderServices_Update]");
                vDBHelper.mAddParameter("@ProviderID", SqlDbType.BigInt, ParameterDirection.Input, ProviderID);
                vDBHelper.mAddParameter("@MOUID", SqlDbType.Int, ParameterDirection.Input, MOUID);
                vDBHelper.mAddParameter("@ProviderTariff", SqlDbType.Structured, ParameterDirection.Input, dt);
                vDBHelper.mAddParameter("@CreatedUserRegionID", SqlDbType.Int, ParameterDirection.Input, CreatedUserRegionID);
                vDBHelper.mAddParameter("@Msg", SqlDbType.VarChar, 1000, ParameterDirection.Output, "");
                DataSet ds = vDBHelper.ExecutewithSingleparam(out strReturn, "@Msg");
                return ds;
            }
            catch (Exception Ex)
            {
                // return "Failed" + Ex.Message;
                throw;
            }
        }

        public DataSet UpdateProviderTariffDiscount(int Serviceid, decimal IPDisscount, decimal OPDisscount, long ProviderID, long MOUID, long CreatedUserRegionID, out string strReturn)
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("ServiceID", typeof(int));
                dt.Columns.Add("FacilityID", typeof(int));
                dt.Columns.Add("Amount", typeof(decimal));
                dt.Columns.Add("Discount", typeof(decimal));
                dt.Columns.Add("UnitType", typeof(int));
                dt.Columns.Add("EffectiveDate", typeof(string));

                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "[USP_ProviderServicesDisscounts_Update]");
                vDBHelper.mAddParameter("@ProviderID", SqlDbType.BigInt, ParameterDirection.Input, ProviderID);
                vDBHelper.mAddParameter("@MOUID", SqlDbType.Int, ParameterDirection.Input, MOUID);
                vDBHelper.mAddParameter("@ServiceID", SqlDbType.Int, ParameterDirection.Input, Serviceid);
                vDBHelper.mAddParameter("@IPDisscount", SqlDbType.Decimal, ParameterDirection.Input, IPDisscount);
                vDBHelper.mAddParameter("@OPDisscount", SqlDbType.Decimal, ParameterDirection.Input, OPDisscount);
                vDBHelper.mAddParameter("@CreatedUserRegionID", SqlDbType.Int, ParameterDirection.Input, CreatedUserRegionID);
                vDBHelper.mAddParameter("@Msg", SqlDbType.VarChar, 1000, ParameterDirection.Output, "");

                DataSet ds = vDBHelper.ExecutewithSingleparam(out strReturn, "@Msg");
                return ds;
            }
            catch (Exception Ex)
            {
                // return "Failed" + Ex.Message;
                throw;
            }
        }

        public DataSet UpdateProviderPackageDiscount(decimal Disscount, long ProviderID, long MOUID, long CreatedUserRegionID, out string strReturn)
        {
            try
            {
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "[USP_ProviderPackageDisscounts_Update]");
                vDBHelper.mAddParameter("@ProviderID", SqlDbType.BigInt, ParameterDirection.Input, ProviderID);
                vDBHelper.mAddParameter("@MOUID", SqlDbType.Int, ParameterDirection.Input, MOUID);
                vDBHelper.mAddParameter("@Disscount", SqlDbType.Decimal, ParameterDirection.Input, Disscount);
                vDBHelper.mAddParameter("@CreatedUserRegionID", SqlDbType.Int, ParameterDirection.Input, CreatedUserRegionID);
                vDBHelper.mAddParameter("@Msg", SqlDbType.VarChar, 1000, ParameterDirection.Output, "");

                DataSet ds = vDBHelper.ExecutewithSingleparam(out strReturn, "@Msg");
                return ds;
            }
            catch (Exception Ex)
            {
                // return "Failed" + Ex.Message;
                throw;
            }
        }
        #endregion

        #region For Package form - HyperlinkClick
        public DataTable GetPakageHistory(string PropertyID, string FacilityID, string TPAProcID, string MOUTypeID, string MOUEntityID)
        {
            try
            {
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "USP_ProviderHistory");
                vDBHelper.mAddParameter("@ProvReqID", SqlDbType.BigInt, ParameterDirection.Input, PropertyID);
                if (FacilityID != "0")
                    vDBHelper.mAddParameter("@FacilityID", SqlDbType.Int, ParameterDirection.Input, FacilityID);

                if (TPAProcID != "0")
                    vDBHelper.mAddParameter("@TPAProcID", SqlDbType.Int, ParameterDirection.Input, TPAProcID);

                vDBHelper.mAddParameter("@MOUTypeID", SqlDbType.Int, ParameterDirection.Input, MOUTypeID);
                vDBHelper.mAddParameter("@MOUEntityID", SqlDbType.Int, ParameterDirection.Input, MOUEntityID);
                DataSet ds = new DataSet();
                ds = vDBHelper.Execute();

                DataTable NewTable = new DataTable();
                NewTable.Columns.Add("TPAProcID");
                NewTable.Columns.Add("Discount", typeof(int));
                NewTable.Columns.Add("LOS");
                NewTable.Columns.Add("EffectiveDate");
                NewTable.Columns.Add("Stage");
                NewTable.Columns.Add("Inclusions");
                NewTable.Columns.Add("Exclusions");
                NewTable.Columns.Add("facValues");

                DataTable InnerTable = new DataTable();
                InnerTable.Columns.Add("FacilityID");
                InnerTable.Columns.Add("Amount", typeof(int));

                DataTable dt = new DataTable();
                dt = ds.Tables[0];
                dt.DefaultView.Sort = "EffectiveDate";
                DataTable dt1 = dt.DefaultView.ToTable();//"TPAProcID";
                string facValues = "";

                for (int i = 0; i < dt1.Rows.Count; i++)
                {
                    DataRow dr = dt1.Rows[i];
                    DataRow nextRow = dt1.Rows[i];
                    if (i != dt1.Rows.Count - 1)
                        nextRow = dt1.Rows[i + 1];
                    else
                    {
                        if (InnerTable.Rows.Count > 0)
                        {
                            DataRow d = InnerTable.Rows[InnerTable.Rows.Count - 1];
                            if (dr["FacilityID"].ToString() != d["FacilityID"].ToString())
                                InnerTable.Rows.Add(dr["FacilityID"], dr["Amount"]);
                        }
                        else InnerTable.Rows.Add(dr["FacilityID"], dr["Amount"]);
                        facValues = Newtonsoft.Json.JsonConvert.SerializeObject(InnerTable);
                        NewTable.Rows.Add(dr["TPAProcID"], dr["Discount"], dr["LOS"], dr["EffectiveDate"], dr["Stage"], dr["Inclusions"], dr["Exclusions"], facValues);
                    }

                    if (dr["EffectiveDate"].ToString() == nextRow["EffectiveDate"].ToString())
                    {
                        if (InnerTable.Rows.Count > 0)
                        {
                            DataRow d = InnerTable.Rows[InnerTable.Rows.Count - 1];
                            if (dr["FacilityID"].ToString() != d["FacilityID"].ToString())
                                InnerTable.Rows.Add(dr["FacilityID"], dr["Amount"]);
                        }
                        else
                            InnerTable.Rows.Add(dr["FacilityID"], dr["Amount"]);
                    }
                    else
                    {

                        if (InnerTable.Rows.Count > 0)
                        {
                            DataRow d = InnerTable.Rows[InnerTable.Rows.Count - 1];
                            if (dr["FacilityID"].ToString() != d["FacilityID"].ToString())
                                InnerTable.Rows.Add(dr["FacilityID"], dr["Amount"]);
                        }
                        else
                            InnerTable.Rows.Add(dr["FacilityID"], dr["Amount"]);
                        facValues = Newtonsoft.Json.JsonConvert.SerializeObject(InnerTable);
                        InnerTable.Clear();
                        NewTable.Rows.Add(dr["TPAProcID"], dr["Discount"], dr["LOS"], dr["EffectiveDate"], dr["Stage"], dr["Inclusions"], dr["Exclusions"], facValues);
                    }
                }
                return NewTable;
            }
            catch (Exception Ex)
            {
                throw;
            }
        }
        #endregion

        #region Package Final Confirm
        public DataSet PackageFinalConfirm(long ProvReqID, string CreatedUserRegionID, string isfinal, long DMSID, string CommunicationRemarks, out string strReturn)
        {
            try
            {
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "USP_ProReqRates_Final");
                vDBHelper.mAddParameter("@ProvReqID", SqlDbType.BigInt, ParameterDirection.Input, ProvReqID);
                vDBHelper.mAddParameter("@CreatedUserRegionID", SqlDbType.Int, ParameterDirection.Input, CreatedUserRegionID);
                vDBHelper.mAddParameter("@isfinal", SqlDbType.Bit, ParameterDirection.Input, isfinal);
                vDBHelper.mAddParameter("@DMSID", SqlDbType.BigInt, ParameterDirection.Input, DMSID);
                vDBHelper.mAddParameter("@CommunicationRemarks", SqlDbType.VarChar, ParameterDirection.Input, CommunicationRemarks);
                vDBHelper.mAddParameter("@Msg", SqlDbType.VarChar, 1000, ParameterDirection.Output, "");

                DataSet ds = vDBHelper.ExecutewithSingleparam(out strReturn, "@Msg");
                return ds;
            }
            catch (Exception Ex)
            {
                // return "Failed" + Ex.Message;
                throw;
            }
        }
        #endregion

        #region Package verification - for stageID=5
        public DataSet PackageVerification(string ProvReqID, string statusID, string CreatedUserRegionID, string Remarks, long DMSID, string CommunicationRemarks, out string strReturn)
        {
            try
            {
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "Usp_ProvReqRates_Verification");
                vDBHelper.mAddParameter("@ProviderReqID", SqlDbType.BigInt, ParameterDirection.Input, ProvReqID);
                vDBHelper.mAddParameter("@statusid", SqlDbType.Int, ParameterDirection.Input, statusID);
                vDBHelper.mAddParameter("@CreatedUserRegionID", SqlDbType.Int, ParameterDirection.Input, CreatedUserRegionID);
                vDBHelper.mAddParameter("@Remarks", SqlDbType.VarChar, ParameterDirection.Input, Remarks);
                vDBHelper.mAddParameter("@DMSID", SqlDbType.BigInt, ParameterDirection.Input, DMSID);
                vDBHelper.mAddParameter("@CommunicationRemarks", SqlDbType.VarChar, ParameterDirection.Input, CommunicationRemarks);
                vDBHelper.mAddParameter("@Msg", SqlDbType.VarChar, 1000, ParameterDirection.Output, "");

                DataSet ds = vDBHelper.ExecutewithSingleparam(out strReturn, "@Msg");
                return ds;
            }
            catch (Exception Ex)
            {
                // return "Failed" + Ex.Message;
                throw;
            }
        }
        #endregion

        #endregion

        #region PRN Functionalities
        public DataTable GetALLProviderPRNDetails(Int32 ProviderID)
        {
            try
            {
                DataTable dt = new DataTable();
                vDBHelper.mCreateCommand(CommandType.Text, "select PP.Id, misa.Name as MstInsurerName, pp.PRNnumber, pp.Remarks,pp.Insurer_Id  from ProviderPRN pp, Mst_IssuingAuthority misa where pp.Insurer_Id = misa.ID and pp.Provider_Id = @ProviderID  and misa.Deleted = 0 and pp.Deleted = 0   ORDER BY PP.ID DESC");
                vDBHelper.mAddParameter("@ProviderID", SqlDbType.Int, ParameterDirection.Input, ProviderID);
                dt = vDBHelper.ExecuteDT();
                return dt;
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
        }

        public DataTable GetProviderPRNValidation(string PRNNumber, int _IssuId)
        {
            try
            {
                DataTable dt = new DataTable();
                vDBHelper.mCreateCommand(CommandType.Text, "select * from ProviderPRN where  Insurer_Id=@IssueId  AND PRNNUMBER=@PRNNumber");
                vDBHelper.mAddParameter("@IssueId", SqlDbType.Int, ParameterDirection.Input, _IssuId);
                vDBHelper.mAddParameter("@PRNNumber", SqlDbType.VarChar, 200, ParameterDirection.Input, PRNNumber);
                dt = vDBHelper.ExecuteDT();
                return dt;
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
        }

        public DataTable PRNUpdateforExistingInsurer(int _ProviderId, int _IssuId)
        {
            try
            {
                DataTable dt = new DataTable();
                vDBHelper.mCreateCommand(CommandType.Text, "select * from ProviderPRN where Provider_Id=@ProviderId  AND  Insurer_Id=@IssueId");
                vDBHelper.mAddParameter("@ProviderId", SqlDbType.Int, ParameterDirection.Input, _ProviderId);
                vDBHelper.mAddParameter("@IssueId", SqlDbType.Int, ParameterDirection.Input, _IssuId);
                dt = vDBHelper.ExecuteDT();
                return dt;
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
        }

        public string InsertProviderPRN(ProviderPRN objRPDetails)
        {
            try
            {
                string strReturn;
                int Mouid = objRPDetails.Mou_id != null ? Convert.ToInt32(objRPDetails.Mou_id) : 0;
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "Usp_ProviderPRN_Insert");
                vDBHelper.mAddParameter("@ProviderID", SqlDbType.Int, ParameterDirection.Input, objRPDetails.Provider_Id);
                vDBHelper.mAddParameter("@MOUID", SqlDbType.Int, ParameterDirection.Input, Mouid);
                vDBHelper.mAddParameter("@PRNNumber", SqlDbType.VarChar, 200, ParameterDirection.Input, objRPDetails.PRNnumber);
                vDBHelper.mAddParameter("@InsurerId", SqlDbType.Int, ParameterDirection.Input, objRPDetails.Insurer_Id);
                vDBHelper.mAddParameter("@Remarks", SqlDbType.VarChar, 200, ParameterDirection.Input, objRPDetails.Remarks);
                vDBHelper.mAddParameter("@CreationDateTime", SqlDbType.DateTime, ParameterDirection.Input, objRPDetails.CreatedDate);
                vDBHelper.mAddParameter("@ModifyDateTime", SqlDbType.DateTime, ParameterDirection.Input, objRPDetails.ModifiedDate);
                vDBHelper.mAddParameter("@Deleted", SqlDbType.Bit, ParameterDirection.Input, objRPDetails.Deleted);
                vDBHelper.mAddParameter("@Msg", SqlDbType.VarChar, 1000, ParameterDirection.Output, "");
                vDBHelper.ExecuteNonQuery(out strReturn, "@Msg");
                return strReturn;
            }
            catch (Exception Ex)
            {
                return "Failed" + Ex.Message;
                throw;
            }
        }

        public DataTable GetProviderPRN(Int32? Id)
        {
            try
            {
                DataTable dt = new DataTable();
                vDBHelper.mCreateCommand(CommandType.Text, " select PRN.Id,MI.Name as MstInsurerName,PRN.PRNnumber,PRN.Remarks,PRN.Insurer_Id from ProviderPRN PRN INNER JOIN Mst_IssuingAuthority MI ON PRN.Insurer_Id = MI.ID WHERE PRN.Provider_Id = @Id Order By PRN.Id DESC");
                vDBHelper.mAddParameter("@Id", SqlDbType.BigInt, ParameterDirection.Input, Id);
                dt = vDBHelper.ExecuteDT();
                return dt;
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
        }

        public string UpdateProviderPRN(ProviderPRN objRPDetails)
        {
            try
            {
                string strReturn;
                //vDBHelper.mCreateCommand(CommandType.StoredProcedure, "Usp_ProviderPRN_AuditInsert");
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "Usp_ProviderPRN_Update");
                vDBHelper.mAddParameter("@ID", SqlDbType.Int, ParameterDirection.Input, objRPDetails.Id);
                vDBHelper.mAddParameter("@ProviderID", SqlDbType.Int, ParameterDirection.Input, objRPDetails.Provider_Id);
                //vDBHelper.mAddParameter("@MOUID", SqlDbType.Int, ParameterDirection.Input, objRPDetails.Mou_id);
                vDBHelper.mAddParameter("@PRNNumber", SqlDbType.VarChar, 200, ParameterDirection.Input, objRPDetails.PRNnumber);
                vDBHelper.mAddParameter("@InsurerId", SqlDbType.Int, ParameterDirection.Input, objRPDetails.Insurer_Id);
                vDBHelper.mAddParameter("@Remarks", SqlDbType.VarChar, 200, ParameterDirection.Input, objRPDetails.Remarks);
                //vDBHelper.mAddParameter("@CreationDateTime", SqlDbType.DateTime, ParameterDirection.Input, objRPDetails.CreatedDate);
                vDBHelper.mAddParameter("@ModifyDateTime", SqlDbType.DateTime, ParameterDirection.Input, objRPDetails.ModifiedDate);
                vDBHelper.mAddParameter("@Deleted", SqlDbType.Bit, ParameterDirection.Input, objRPDetails.Deleted);
                vDBHelper.mAddParameter("@Msg", SqlDbType.VarChar, 1000, ParameterDirection.Output, "");
                vDBHelper.ExecuteNonQuery(out strReturn, "@Msg");
                return strReturn;
            }
            catch (Exception Ex)
            {
                return "Failed" + Ex.Message;
                throw;
            }
        }

        public string UpdateProviderPRNExistingInsurer(string PRNNumber, int? PRNID, DateTime ModifyDate, string Remarks)
        {
            try
            {
                DataTable dt = new DataTable();
                vDBHelper.mCreateCommand(CommandType.Text, "update ProviderPRN SET PRNnumber=@PRNNumber,ModifiedDate=@ModifyDate,Remarks=@Remarks where id=@PRNID");
                vDBHelper.mAddParameter("@PRNNumber", SqlDbType.VarChar, 200, ParameterDirection.Input, PRNNumber);
                vDBHelper.mAddParameter("@PRNID", SqlDbType.Decimal, ParameterDirection.Input, PRNID);
                vDBHelper.mAddParameter("@ModifyDate", SqlDbType.DateTime, ParameterDirection.Input, ModifyDate);
                vDBHelper.mAddParameter("@Remarks", SqlDbType.VarChar, ParameterDirection.Input, Remarks);
                dt = vDBHelper.ExecuteDT();
                return "";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string UpdateProviderPRN(string PRNNumber, string Remarks, int? PrnId, int isurerId)
        {
            try
            {
                DataTable dt = new DataTable();
                vDBHelper.mCreateCommand(CommandType.Text, "update  ProviderPRN set PRNnumber=@PRNNumber, Remarks=@Remarks,Insurer_Id=@INSURERId  where Id=@PNRID");
                vDBHelper.mAddParameter("@PRNNumber", SqlDbType.VarChar, 200, ParameterDirection.Input, PRNNumber);
                vDBHelper.mAddParameter("@Remarks", SqlDbType.VarChar, 200, ParameterDirection.Input, Remarks);
                vDBHelper.mAddParameter("@PNRID", SqlDbType.Int, ParameterDirection.Input, PrnId);
                vDBHelper.mAddParameter("@INSURERId", SqlDbType.Int, ParameterDirection.Input, isurerId);
                dt = vDBHelper.ExecuteDT();
                return "";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region GetProviderBankDetails(Changes done for Task: SP-977)
        public DataTable GetProviderBankDetails(Int64 ProviderID, string AccountNumber, int BankID, int ApprovalStatus)
        {
            try
            {
                if (AccountNumber == "0")
                {
                    AccountNumber = "";
                }
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "USP_ProviderBankDetails_Insurer");
                vDBHelper.mAddParameter("@ProviderID", SqlDbType.BigInt, ParameterDirection.Input, ProviderID);
                vDBHelper.mAddParameter("@ApprovalStatus", SqlDbType.TinyInt, ParameterDirection.Input, ApprovalStatus);
                vDBHelper.mAddParameter("@AccountNumber", SqlDbType.VarChar, ParameterDirection.Input, AccountNumber);
                vDBHelper.mAddParameter("@BankID", SqlDbType.Int, ParameterDirection.Input, BankID);
                DataTable dt = vDBHelper.ExecuteDT();
                return dt;
            }
            catch (Exception Ex)
            {
                string errorMag = Ex.Message;
                return new DataTable();
                //throw
            }
        }
        #endregion

        //public DataTable GetProviderBankDetailsByBankID(Int64 BankID)
        //{
        //    try
        //    {
        //        vDBHelper.mCreateCommand(CommandType.Text, "select B.ID,ProviderID,B.Name BankName,B.Branch,B.IFSCCode,B.MICRCode,PB.AccountNumber,AccountTypeID,Payeename,EffectiveDate,act.name as accounttype,Emailid,SecEmailID,MobileNo,AltMobileNo,Createddatetime,CreatedUserRegionID,InsurerID "
        //        + " from ProviderBankDetails PB,MSt_Bank B,Mst_AccountType act "
        //       + "where PB.BankID=B.ID and PB.Deleted=0 and PB.BankID=@BankID and act.id=AccountTypeID GROUP BY B.ID,ProviderID,B.Name,B.Branch,B.IFSCCode,B.MICRCode,PB.AccountNumber,AccountTypeID,Payeename, "
        //        + " EffectiveDate,act.name,Emailid,SecEmailID,MobileNo,AltMobileNo,Createddatetime,CreatedUserRegionID,InsurerID ");
        //        vDBHelper.mAddParameter("@BankID", SqlDbType.BigInt, ParameterDirection.Input, BankID);
        //        DataTable dt = vDBHelper.ExecuteDT();
        //        return dt;
        //    }
        //    catch (Exception Ex)
        //    {
        //        throw;
        //    } 
        //}

        // Begin SP3V-2400 Modified By Meena to get the records that has Provider status ID Not Null
        public DataTable GetProviderFlagDetails(Int64 ProviderID)
        {
            try
            {
                vDBHelper.mCreateCommand(CommandType.Text, "select * from ProviderCategory with (nolock) where ProviderID=@ProviderID and providerStatusID is not null order by id desc");
                vDBHelper.mAddParameter("@ProviderID", SqlDbType.BigInt, ParameterDirection.Input, ProviderID);
                DataTable dt = vDBHelper.ExecuteDT();
                return dt;
            }
            catch (Exception Ex)
            {
                throw;
            }
        }

        // Begin SP3V-5141 
        public DataTable GetProviderFlagDetailsInsurerSpecific(Int64 ProviderID, int UserID)
        {
            try
            {
                vDBHelper.mCreateCommand(CommandType.Text, "select * from ProviderCategory with (nolock) where ProviderID=@ProviderID and providerStatusID is not null and RelevantID in (Select distinct IssueID from lnk_UserIssuingAuthority with (nolock) where Deleted = 0 and UserID = @UserID) order by id desc");
                vDBHelper.mAddParameter("@ProviderID", SqlDbType.BigInt, ParameterDirection.Input, ProviderID);
                vDBHelper.mAddParameter("@UserID", SqlDbType.Int, ParameterDirection.Input, UserID);
                DataTable dt = vDBHelper.ExecuteDT();
                return dt;
            }
            catch (Exception Ex)
            {
                throw;
            }
        }

        // Begin SP3V-2400 Modified By Meena to get the records that has Provider status ID Not Null
        public DataTable GetProviderFlagDetailsWithInsurerSpecific(Int64 ProviderID, int UserID)
        {
            try
            {

                vDBHelper.mCreateCommand(CommandType.Text, "select * from ProviderCategory PC inner join lnk_Userissuingauthority UI on PC.RelevantID = UI.IssueID and UI.Deleted = 0" +
                            "where ProviderID = @ProviderID and providerStatusID is not null and UI.userid = @UserID order by PC.id desc");
                vDBHelper.mAddParameter("@ProviderID", SqlDbType.BigInt, ParameterDirection.Input, ProviderID);
                vDBHelper.mAddParameter("@UserID", SqlDbType.BigInt, ParameterDirection.Input, UserID);
                DataTable dt = vDBHelper.ExecuteDT();
                return dt;
            }
            catch (Exception Ex)
            {
                throw;
            }
        }
        // End SP3V-2400

        public DataTable GetProviderPRNDetails(int _PRNId)
        {
            try
            {
                DataTable dt = new DataTable();
                vDBHelper.mCreateCommand(CommandType.Text, "SELECT * fROM ProviderPRN WHERE Id=@PRNID ");
                vDBHelper.mAddParameter("@PRNID", SqlDbType.Int, ParameterDirection.Input, _PRNId);
                dt = vDBHelper.ExecuteDT();
                return dt;
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
        }

        public DataTable GetProviderReqBankDetails(string ProvReqID)
        {
            try
            {
                string qryProReqBank = "SELECT B.ID,ProviderReqID,B.Name AS BankName,B.Branch,B.IFSCCode,B.MICRCode,PB.AccountNumber,AccountTypeID,Payeename" +
                                       ", EffectiveDate, act.name AS accounttype FROM ProviderReqBankDetails PB, MSt_Bank B,Mst_AccountType act" +
                                       " WHERE PB.BankID = B.ID AND PB.Deleted = 0 AND PB.ProviderReqID = @ProvReqID AND act.id = AccountTypeID;";
                vDBHelper.mCreateCommand(CommandType.Text, qryProReqBank);
                vDBHelper.mAddParameter("@ProvReqID", SqlDbType.BigInt, ParameterDirection.Input, ProvReqID);
                DataTable dt = vDBHelper.ExecuteDT();
                return dt;
            }
            catch (Exception Ex)
            {
                throw;
            }
        }

        public DataTable GetProviderReqEarlyPaymentDisscounts(string ProvReqID)
        {
            try
            {
                vDBHelper.mCreateCommand(CommandType.Text, "select ID, ProviderReqID,NoofDays,Percentage,EffectiveFrom " +
                    " from ProviderReqEarlyPayDiscount where Deleted=0 and ProviderReqID=@ProvReqID");
                vDBHelper.mAddParameter("@ProvReqID", SqlDbType.BigInt, ParameterDirection.Input, ProvReqID);
                DataTable dt = vDBHelper.ExecuteDT();
                return dt;
            }
            catch (Exception Ex)
            {
                throw;
            }
        }

        public DataTable GetProviderReqMOUDetails(string ProvReqID)
        {
            try
            {
                vDBHelper.mCreateCommand(CommandType.Text, "select ID,ProviderReqID,MOUTypeID_P44,MOUEntityID,MOUNo,StartDate,EndDate,ExpiryDate,MOUDiscountPerc,MOUDiscountAbs,AgreementType from ProviderReqMOU M " +
                    " where ProviderReqID=@ProvReqID and M.Deleted=0");
                vDBHelper.mAddParameter("@ProvReqID", SqlDbType.BigInt, ParameterDirection.Input, ProvReqID);
                DataTable dt = vDBHelper.ExecuteDT();
                return dt;
            }
            catch (Exception Ex)
            {
                string error = Ex.Message;
                throw;
            }
        }

        public DataSet GetMouMasters(string ProvReqID)
        {
            try
            {
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "USP_LoadMOUData");
                vDBHelper.mAddParameter("@ProvReqID", SqlDbType.BigInt, ParameterDirection.Input, ProvReqID);
                DataSet ds = vDBHelper.Execute();

                vDBHelper.mCreateCommand(CommandType.Text, "select id,name from Mst_AccountType where Deleted=0");
                vDBHelper.mAddParameter("@ProvReqID", SqlDbType.BigInt, ParameterDirection.Input, ProvReqID);
                DataTable dt = vDBHelper.ExecuteDT();

                ds.Tables.Add(dt);
                //1st result Set- MOU details  2nd-Bank details 3rd-TDS details 4th-Early Payment discounts 5th- Account type DropDown
                return ds;
            }
            catch (Exception Ex)
            {
                string error = Ex.Message;
                throw;
            }
        }

        public DataTable GetProviderDiscount(long ProviderReqID)
        {
            try
            {
                string qryProvReqDiscount = "SELECT MOUTypeID_P44,MOUEntityID,PackageDiscount,IPDiscount,OPDiscount,"
                                            + "REPLACE(CONVERT(VARCHAR(15),EffectiveDate, 106), ' ', '-') AS EffectiveDate "
                                            + "FROM ProviderReqDiscountPercentages WHERE ProviderReqID = @ProviderReqID AND Deleted = 0;";
                vDBHelper.mCreateCommand(CommandType.Text, qryProvReqDiscount);
                vDBHelper.mAddParameter("@ProviderReqID", SqlDbType.BigInt, ParameterDirection.Input, ProviderReqID);
                DataTable dt = vDBHelper.ExecuteDT();
                return dt;
            }
            catch (Exception Ex)
            {
                string error = Ex.Message;
                throw;
            }
        }

        public List<DAL.Mst_Services> GetProviderServices()
        {
            try
            {
                using (var context = new DAL.McarePlusEntities())
                {

                    List<int> SS = new List<int>() { 1, 7, 20, 33, 42, 13, 25, 49 };

                    var list = context.Mst_Services.ToList().Where(t => SS.Contains(t.ID)).ToList();
                    return list;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public DataTable GetProviderServicesCatergories()
        {
            try
            {

                DataTable dt = new DataTable();
                vDBHelper.mCreateCommand(CommandType.Text, "select ID,Name from mst_services with (nolock) where parentId=0 and Deleted=0");
                dt = vDBHelper.ExecuteDT();
                return dt;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public DataTable GetServicewiseIPOPDiscount(string MOUID, long PROVIDERID, DateTime? EfDate)
        {
            try
            {

                DataTable dt = new DataTable();
                // vDBHelper.mCreateCommand(CommandType.Text, "select DISTINCT s.id as ServiceID,s.SERVICECODE as Category,s.Name,ps.Name as ParentName,isnull (pv.Name,'') as UnitType,isnull(pt.Discount,isnull((select IPPercentage from ProviderMOU where ProviderID=229976 and ID=1162262),0)) as IPDiscount,isnull(pt.OPDiscount,isnull((select OPPercentage from ProviderMOU where ProviderID=229976 and ID=1162262),0)) as OPDiscount ,pt.Amount ,CONVERT(VARCHAR(11), COALESCE(pt.EffectiveDate, pt.CreatedDatetime), 105) EffectiveDate,isnull(pt.Remarks,'') as Remarks ,'<span >Current</span>' as Status from Mst_Services s  inner join Mst_Services ps on ps.ID = s.ParentID  left join Mst_PropertyValues pv on pv.ID = s.UnitType_P67   left join ProviderTariff pt on pt.Serviceid = s.ID and pt.Deleted=0   where s.deleted = 0 and s.ParentID != 0 and (pt.OPDiscount >0 or pt.Discount>0)  and s.IProviderServices = 1  and pt.ProviderID =229976 and   pt.MOUID=1162262 and isLatest=1  order by s.ID");
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "[Usp_GetExclusiveServiceDiscount]");

                vDBHelper.mAddParameter("@MOUID", SqlDbType.VarChar, ParameterDirection.Input, MOUID);

                vDBHelper.mAddParameter("@PROVIDERID", SqlDbType.BigInt, ParameterDirection.Input, PROVIDERID);
                vDBHelper.mAddParameter("@EffectiveDate", SqlDbType.DateTime, ParameterDirection.Input, EfDate == null ? (object)DBNull.Value : Convert.ToDateTime(EfDate));
                dt = vDBHelper.ExecuteDT();
                return dt;
            }
            catch (Exception Ex)
            {
                throw;
            }

        }
        public DataSet InsertandupdateServiceWiseDiscount(DataTable servicesDiscountDetails, Int64 _providerID, int CreatedUserRegionID)
        {
            try
            {
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "Usp_ServiceSpecific_ProviderTariff_BulkUpdate");
                vDBHelper.mAddParameter("@ServiceDiscounts", SqlDbType.Structured, ParameterDirection.Input, servicesDiscountDetails);
                vDBHelper.mAddParameter("@ProviderID", SqlDbType.BigInt, ParameterDirection.Input, _providerID);
                vDBHelper.mAddParameter("@CreatedUserRegionID", SqlDbType.BigInt, ParameterDirection.Input, CreatedUserRegionID);
                vDBHelper.mAddParameter("@Msg", SqlDbType.VarChar, 1000, ParameterDirection.Output, DBNull.Value);
                //string strReturn;
                //vDBHelper.ExecuteNonQuery(out strReturn, "@Msg");
                //return strReturn;
                DataSet ds = vDBHelper.Execute();
                return ds;
            }
            catch (Exception Ex)
            {
                //return "Failed" + Ex.Message;
                throw;
            }
        }
        //public DataTable GetServicewiseIPOPDiscount(long MOUID, long PROVIDERID)
        //{
        //    try
        //    {
        //        DataTable dt = new DataTable();
        //        vDBHelper.mCreateCommand(CommandType.Text, "select  s.id as ServiceID, s.SERVICECODE as Category, s.Name, ps.Name as ParentName, isnull (pv.Name,'') as UnitType, isnull(pt.Discount,0) as IPDiscount, isnull(pt.OPDiscount,0) as OPDiscount , mc.Level2, pt.Amount , CONVERT(VARCHAR(11), COALESCE(pt.EffectiveDate, pt.CreatedDatetime), 105) EffectiveDate,isnull(pt.Remarks,'') Remarks from Mst_Services s  inner join Mst_Services ps on ps.ID = s.ParentID  left join Mst_PropertyValues pv on pv.ID = s.UnitType_P67   left join ProviderTariff pt on pt.Serviceid = s.ID and pt.Deleted=0   and pt.FacilityID is not null  left join Mst_Facility mc on mc.id = pt.FacilityID   where s.deleted = 0 and s.ParentID != 0 and (pt.OPDiscount >0 or pt.Discount>0)  and s.IProviderServices = 1   and pt.ProviderID =@PROVIDERID and   pt.MOUID=@MOUID and isLatest=1    and (Discount<>isnull((select IPPercentage from ProviderMOU where ProviderID=@PROVIDERID   and ID=@MOUID),0) or OPDiscount<>isnull((select OPPercentage from ProviderMOU where ProviderID=@PROVIDERID and ID=@MOUID),0))   order by s.ID");
        //        vDBHelper.mAddParameter("@MOUID", SqlDbType.BigInt, ParameterDirection.Input, MOUID);

        //        vDBHelper.mAddParameter("@PROVIDERID", SqlDbType.BigInt, ParameterDirection.Input, PROVIDERID);
        //        dt = vDBHelper.ExecuteDT();
        //        return dt;
        //    }
        //    catch (Exception Ex)
        //    {
        //        throw;
        //    }

        //}
        #region Insert and Update MOU

        public DataSet GetProviderDMSData(long ProviderReqID)
        {
            try
            {
                vDBHelper.mCreateCommand(CommandType.Text, "select distinct ServiceTAXNoDMSID,RegistrationNoDMSID,CancelledChequeDMSID,NEFTDeclarationDMSID,PayeeDeclarationDMSID ,HospitalProfarmaDMSID,MOUDMSID," +
                    "ServiceTAXFileName= case when  ServiceTAXNoDMSID>0 then (select top 1 name from DMSFileinfo_Provider where id=ServiceTAXNoDMSID) end," +
                    "RegistrationNoFileName= case when  RegistrationNoDMSID>0 then (select top 1 name from DMSFileinfo_Provider where id=RegistrationNoDMSID) end," +
                    "CancelledChequeFileName= case when  CancelledChequeDMSID>0 then (select top 1 name from DMSFileinfo_Provider where id=CancelledChequeDMSID) end," +
                    "NEFTDeclarationFileName= case when  NEFTDeclarationDMSID>0 then (select top 1 name from DMSFileinfo_Provider where id=NEFTDeclarationDMSID) end," +
                    "PayeeDeclarationFileName= case when  PayeeDeclarationDMSID>0 then (select top 1 name from DMSFileinfo_Provider where id=PayeeDeclarationDMSID) end," +
                    "HospitalProfarmaFileName= case when  HospitalProfarmaDMSID>0 then (select top 1 name from DMSFileinfo_Provider where id=HospitalProfarmaDMSID) end," +
                    "MOUFileName= case when  MOUDMSID>0 then (select top 1 name from DMSFileinfo_Provider where id=MOUDMSID) end" +
                    " from ProviderReq p where p.ID=@ProviderReqID" +
                    " select f.Level1,p.DMSID,dm.Name from ProviderReqFacilities p inner join Mst_Facility f on p.FacilityID=f.ID inner join DMSFileinfo_Provider dm on dm.ID=p.DMSID" +
                    " where ProviderReqID=@ProviderReqID and p.Deleted=0 and f.Deleted=0 group by f.Level1,p.DMSID,dm.Name"
                    );
                vDBHelper.mAddParameter("@ProviderReqID", SqlDbType.BigInt, ParameterDirection.Input, ProviderReqID);
                DataSet ds = vDBHelper.Execute();
                return ds;
            }
            catch (Exception Ex)
            {
                throw;
            }
        }

        public DataSet GetProviderDMSData_Internal(long ProviderID, long MOUID)
        {
            try
            {
                vDBHelper.mCreateCommand(CommandType.Text, "select distinct ServiceTAXNoDMSID,RegistrationNoDMSID,CancelledChequeDMSID,NEFTDeclarationDMSID,PayeeDeclarationDMSID ,HospitalProfarmaDMSID,MOUDMSID," +
                    "ServiceTAXFileName= case when  ServiceTAXNoDMSID>0 then (select top 1 name from DMSFileinfo_Provider where id=ServiceTAXNoDMSID) end," +
                    "RegistrationNoFileName= case when  RegistrationNoDMSID>0 then (select top 1 name from DMSFileinfo_Provider where id=RegistrationNoDMSID) end," +
                    "CancelledChequeFileName= case when  CancelledChequeDMSID>0 then (select top 1 name from DMSFileinfo_Provider where id=CancelledChequeDMSID) end," +
                    "NEFTDeclarationFileName= case when  NEFTDeclarationDMSID>0 then (select top 1 name from DMSFileinfo_Provider where id=NEFTDeclarationDMSID) end," +
                    "PayeeDeclarationFileName= case when  PayeeDeclarationDMSID>0 then (select top 1 name from DMSFileinfo_Provider where id=PayeeDeclarationDMSID) end," +
                    "HospitalProfarmaFileName= case when  HospitalProfarmaDMSID>0 then (select top 1 name from DMSFileinfo_Provider where id=HospitalProfarmaDMSID) end," +
                    "MOUFileName= case when  MOUDMSID>0 then (select top 1 name from DMSFileinfo_Provider where id=isnull(pm.DMSID,MOUDMSID)) end" +
                    " from Provider p left join ProviderMOU pm on pm.ProviderID=p.ID and p.ID=@MOUID where p.ID=@ProviderID   " +
                    " select f.Level1,p.DMSID,dm.Name from ProviderFacilities p inner join Mst_Facility f on p.FacilityID=f.ID inner join DMSFileinfo_Provider dm on dm.ID=p.DMSID" +
                    " where ProviderID=@ProviderID and p.Deleted=0 and f.Deleted=0 group by f.Level1,p.DMSID,dm.Name"
                    );
                vDBHelper.mAddParameter("@ProviderID", SqlDbType.BigInt, ParameterDirection.Input, ProviderID);
                vDBHelper.mAddParameter("@MOUID", SqlDbType.BigInt, ParameterDirection.Input, MOUID);
                DataSet ds = vDBHelper.Execute();
                return ds;
            }
            catch (Exception Ex)
            {
                throw;
            }
        }

        public DataSet UpdateServiceDissCounts(long ProviderReqID, int ServiceID, int MOUType, long MouEntityID, Decimal IPDisscount, Decimal OPDisscount)
        {
            try
            {
                vDBHelper.mCreateCommand(CommandType.Text, "update ProviderReqTariff set Discount= @IPDisscount, IPDisscount= @IPDisscount,OPDisscount=@OPDisscount where ProviderReqID=@ProviderReqID and Serviceid=@ServiceID and Deleted=0" +
                    " and MOUTypeID_P44=@MOUType and MOUEntityID=@MouEntityID");
                vDBHelper.mAddParameter("@ProviderReqID", SqlDbType.BigInt, ParameterDirection.Input, ProviderReqID);
                vDBHelper.mAddParameter("@ServiceID", SqlDbType.Int, ParameterDirection.Input, ServiceID);
                vDBHelper.mAddParameter("@MOUType", SqlDbType.Int, ParameterDirection.Input, MOUType);
                vDBHelper.mAddParameter("@MouEntityID", SqlDbType.BigInt, ParameterDirection.Input, MouEntityID);
                vDBHelper.mAddParameter("@IPDisscount", SqlDbType.Decimal, ParameterDirection.Input, IPDisscount);
                vDBHelper.mAddParameter("@OPDisscount", SqlDbType.Decimal, ParameterDirection.Input, OPDisscount);
                DataSet ds = vDBHelper.Execute();
                return ds;
            }
            catch (Exception Ex)
            {
                throw;
            }
        }

        public string UpdateProviderDMSDetails(long ProviderReqID, decimal DMSTypeID, decimal DMSID)
        {
            try
            {
                string strReturn;
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "USP_ProvReqDMSUploads_Insert");
                vDBHelper.mAddParameter("@ProviderReqID", SqlDbType.BigInt, ParameterDirection.Input, ProviderReqID);
                vDBHelper.mAddParameter("@DMSID", SqlDbType.BigInt, ParameterDirection.Input, DMSID);
                vDBHelper.mAddParameter("@DMSTypeID", SqlDbType.Int, ParameterDirection.Input, DMSTypeID);
                vDBHelper.mAddParameter("@Msg", SqlDbType.VarChar, 1000, ParameterDirection.Output, "");
                vDBHelper.ExecuteNonQuery(out strReturn, "@Msg");
                return strReturn;
            }
            catch (Exception Ex)
            {
                return "Failed" + Ex.Message;
                throw;
            }
        }

        public string UpdateProviderDMSDetails_Internal(long ProviderID, string MOUIDs, decimal DMSTypeID, decimal DMSID)
        {
            try
            {
                string strReturn;
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "USP_ProviderDMSUploads_Insert");
                vDBHelper.mAddParameter("@ProviderID", SqlDbType.BigInt, ParameterDirection.Input, ProviderID);
                vDBHelper.mAddParameter("@MOUID", SqlDbType.VarChar, ParameterDirection.Input, MOUIDs);
                vDBHelper.mAddParameter("@DMSID", SqlDbType.BigInt, ParameterDirection.Input, DMSID);
                vDBHelper.mAddParameter("@DMSTypeID", SqlDbType.Int, ParameterDirection.Input, DMSTypeID);
                vDBHelper.mAddParameter("@Msg", SqlDbType.VarChar, 1000, ParameterDirection.Output, "");
                vDBHelper.ExecuteNonQuery(out strReturn, "@Msg");
                return strReturn;
            }
            catch (Exception Ex)
            {
                return "Failed" + Ex.Message;
                throw;
            }
        }

        public string UpdateMOUDiscountPercentages(long ProviderReqID, int MOUType, int MOUEntityID, decimal IPPercentage, decimal OPPercentage, decimal PackagePercentage, string EffectiveDate, int UserRegionID)
        {
            try
            {
                DateTime dd;
                decimal MOUPercentage = IPPercentage;
                if (MOUPercentage > OPPercentage)
                    MOUPercentage = OPPercentage;
                if (MOUPercentage > PackagePercentage)
                    MOUPercentage = PackagePercentage;
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "USP_ProvReqMOUDiscounts_Insert");
                vDBHelper.mAddParameter("@ProviderReqID", SqlDbType.BigInt, ParameterDirection.Input, ProviderReqID);
                vDBHelper.mAddParameter("@MOUType", SqlDbType.Int, ParameterDirection.Input, MOUType);
                vDBHelper.mAddParameter("@MOUEntityID", SqlDbType.Int, ParameterDirection.Input, MOUEntityID);
                if (IPPercentage > 0)
                    vDBHelper.mAddParameter("@IPPercentage", SqlDbType.Decimal, ParameterDirection.Input, IPPercentage);
                if (OPPercentage > 0)
                    vDBHelper.mAddParameter("@OPPercentage", SqlDbType.Decimal, ParameterDirection.Input, OPPercentage);
                if (PackagePercentage > 0)
                    vDBHelper.mAddParameter("@PackagePercentage", SqlDbType.Decimal, ParameterDirection.Input, PackagePercentage);

                //vDBHelper.mAddParameter("@MOUPercentage", SqlDbType.Decimal, ParameterDirection.Input, MOUPercentage);
                if (DateTime.TryParse(EffectiveDate, out dd))
                    vDBHelper.mAddParameter("@EffectiveDate", SqlDbType.DateTime, ParameterDirection.Input, EffectiveDate);
                vDBHelper.mAddParameter("@CreatedUserRegionID", SqlDbType.Int, ParameterDirection.Input, UserRegionID);

                vDBHelper.mAddParameter("@Msg", SqlDbType.VarChar, 1000, ParameterDirection.Output, "");
                string strReturn;
                vDBHelper.ExecuteNonQuery(out strReturn, "@Msg");
                return strReturn;
            }
            catch (Exception Ex)
            {
                return "Failed" + Ex.Message;
                throw;
            }
        }

        public string UpdateMOU(MOU mou)
        {
            try
            {
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "USP_ProvReqMOU_Insert");
                vDBHelper.mAddParameter("@ProviderReqID", SqlDbType.BigInt, ParameterDirection.Input, mou.ProviderReqID);
                vDBHelper.mAddParameter("@ID", SqlDbType.BigInt, ParameterDirection.Output, mou.ID);
                vDBHelper.mAddParameter("@MOUTypeID", SqlDbType.Int, ParameterDirection.Input, mou.MOUTypeID);
                vDBHelper.mAddParameter("@MOUEntityID", SqlDbType.Int, ParameterDirection.Input, mou.MOUEntityID);
                vDBHelper.mAddParameter("@MOUNo", SqlDbType.VarChar, ParameterDirection.Input, mou.MOUNo);
                vDBHelper.mAddParameter("@StartDate", SqlDbType.DateTime, ParameterDirection.Input, mou.StartDate);
                vDBHelper.mAddParameter("@EndDate", SqlDbType.DateTime, ParameterDirection.Input, mou.EndDate);
                vDBHelper.mAddParameter("@DMSID", SqlDbType.Int, ParameterDirection.Input, mou.DMSID);
                vDBHelper.mAddParameter("@CreatedUserRegionID", SqlDbType.Int, ParameterDirection.Input, mou.CreatedUserRegionID);
                vDBHelper.mAddParameter("@Remarks", SqlDbType.VarChar, ParameterDirection.Input, mou.Remarks);
                vDBHelper.mAddParameter("@MOUDiscountPerc", SqlDbType.Decimal, ParameterDirection.Input, mou.MOUDiscountPerc);
                vDBHelper.mAddParameter("@MOUDiscountAbs", SqlDbType.Decimal, ParameterDirection.Input, mou.MOUDiscountAbs);
                if (mou.AgreementID > 0)
                    vDBHelper.mAddParameter("@AgreementID", SqlDbType.BigInt, ParameterDirection.Input, mou.AgreementID);
                if (mou.AgreementType != "")
                    vDBHelper.mAddParameter("@AgreementType", SqlDbType.VarChar, ParameterDirection.Input, mou.AgreementType);
                vDBHelper.mAddParameter("@Msg", SqlDbType.VarChar, 1000, ParameterDirection.Output, "");
                string strReturn;
                long ID;
                vDBHelper.ExecuteNonQuery_Long(out ID, "@ID", out strReturn, "@Msg");
                return strReturn + "_" + ID.ToString();
            }
            catch (Exception Ex)
            {
                return "Failed" + Ex.Message;
                throw;
            }
        }

        public string InsertProviderMOU(long MOUID, ProviderMOU mou)
        {
            try
            {
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "USP_ProviderMOU_Insert");
                vDBHelper.mAddParameter("@MOUID", SqlDbType.BigInt, ParameterDirection.Input, MOUID);
                vDBHelper.mAddParameter("@ProviderID", SqlDbType.BigInt, ParameterDirection.Input, mou.ProviderID);
                vDBHelper.mAddParameter("@MOUTypeID", SqlDbType.Int, ParameterDirection.Input, mou.MOUTypeID);
                vDBHelper.mAddParameter("@MOUEntityID", SqlDbType.Int, ParameterDirection.Input, mou.MOUEntityID);
                vDBHelper.mAddParameter("@MOUNo", SqlDbType.VarChar, ParameterDirection.Input, mou.MOUNo);
                vDBHelper.mAddParameter("@StartDate", SqlDbType.DateTime, ParameterDirection.Input, mou.StartDate);
                vDBHelper.mAddParameter("@EndDate", SqlDbType.DateTime, ParameterDirection.Input, mou.EndDate);
                vDBHelper.mAddParameter("@DMSID", SqlDbType.Int, ParameterDirection.Input, mou.DMSID);
                vDBHelper.mAddParameter("@CreatedUserRegionID", SqlDbType.Int, ParameterDirection.Input, mou.CreatedUserRegionID);
                vDBHelper.mAddParameter("@Remarks", SqlDbType.VarChar, ParameterDirection.Input, mou.Remarks);
                vDBHelper.mAddParameter("@MOUDiscountPerc", SqlDbType.Decimal, ParameterDirection.Input, mou.MOUDiscountPerc);
                vDBHelper.mAddParameter("@MOUDiscountAbs", SqlDbType.Decimal, ParameterDirection.Input, mou.MOUDiscountAbs);
                vDBHelper.mAddParameter("@PRCNO", SqlDbType.VarChar, ParameterDirection.Input, mou.PRCNO);

                vDBHelper.mAddParameter("@Msg", SqlDbType.VarChar, 1000, ParameterDirection.Output, "");
                string strReturn;
                vDBHelper.ExecuteNonQuery(out strReturn, "@Msg");
                return strReturn;
            }
            catch (Exception Ex)
            {
                return "Failed" + Ex.Message;
                throw;
            }
        }

        public string InsertProviderMOUDetails(DAL.ProviderMOU mou, int TempMou = 0)
        {
            try
            {
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "USP_ProviderMOUDetails_Insert");
                vDBHelper.mAddParameter("@MOUID", SqlDbType.BigInt, ParameterDirection.Input, mou.ID);
                vDBHelper.mAddParameter("@ProviderID", SqlDbType.BigInt, ParameterDirection.Input, mou.ProviderID);
                vDBHelper.mAddParameter("@MOUTypeID", SqlDbType.Int, ParameterDirection.Input, mou.MOUTypeID_P44);
                vDBHelper.mAddParameter("@MOUEntityID", SqlDbType.Int, ParameterDirection.Input, mou.MOUEntityID);
                vDBHelper.mAddParameter("@MOUNo", SqlDbType.VarChar, ParameterDirection.Input, mou.MOUNo);
                vDBHelper.mAddParameter("@StartDate", SqlDbType.DateTime, ParameterDirection.Input, mou.StartDate);
                vDBHelper.mAddParameter("@EndDate", SqlDbType.DateTime, ParameterDirection.Input, mou.EndDate);
                vDBHelper.mAddParameter("@CreatedUserRegionID", SqlDbType.Int, ParameterDirection.Input, mou.CreatedUserRegionID);
                vDBHelper.mAddParameter("@PRCNO", SqlDbType.VarChar, ParameterDirection.Input, mou.PRCNo);
                vDBHelper.mAddParameter("@AgreementID", SqlDbType.BigInt, ParameterDirection.Input, mou.AgreementID);
                vDBHelper.mAddParameter("@AgreementType", SqlDbType.VarChar, ParameterDirection.Input, mou.AgreementType);
                vDBHelper.mAddParameter("@Remarks", SqlDbType.VarChar, 500, ParameterDirection.Input, mou.Remarks);
                //SP3V - 3783 Leena
                vDBHelper.mAddParameter("@TempMOU", SqlDbType.Bit, 500, ParameterDirection.Input, TempMou);
                //END
                vDBHelper.mAddParameter("@Msg", SqlDbType.VarChar, 1000, ParameterDirection.Output, "");
                string strReturn;
                vDBHelper.ExecuteNonQuery(out strReturn, "@Msg");
                return strReturn;
            }
            catch (Exception Ex)
            {
                return "Failed" + Ex.Message;
                throw;
            }
        }

        public string UpdateProviderMOUDetails(DAL.ProviderMOU mou, int TempMou = 0)
        {
            try
            {
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "USP_ProviderMOU_Update");
                vDBHelper.mAddParameter("@MOUID", SqlDbType.BigInt, ParameterDirection.Input, mou.ID);
                vDBHelper.mAddParameter("@ProviderID", SqlDbType.BigInt, ParameterDirection.Input, mou.ProviderID);
                vDBHelper.mAddParameter("@MOUTypeID", SqlDbType.Int, ParameterDirection.Input, mou.MOUTypeID_P44);
                vDBHelper.mAddParameter("@MOUEntityID", SqlDbType.Int, ParameterDirection.Input, mou.MOUEntityID);
                vDBHelper.mAddParameter("@MOUNo", SqlDbType.VarChar, ParameterDirection.Input, mou.MOUNo);
                vDBHelper.mAddParameter("@StartDate", SqlDbType.DateTime, ParameterDirection.Input, mou.StartDate);
                vDBHelper.mAddParameter("@EndDate", SqlDbType.DateTime, ParameterDirection.Input, mou.EndDate);
                vDBHelper.mAddParameter("@CreatedUserRegionID", SqlDbType.Int, ParameterDirection.Input, mou.CreatedUserRegionID);
                vDBHelper.mAddParameter("@Remarks", SqlDbType.VarChar, 500, ParameterDirection.Input, mou.Remarks);
                //SP3V - 3783 Leena
                vDBHelper.mAddParameter("@TempMOU", SqlDbType.Bit, 500, ParameterDirection.Input, TempMou);
                //END
                vDBHelper.mAddParameter("@Msg", SqlDbType.VarChar, 1000, ParameterDirection.Output, "");
                string strReturn;
                vDBHelper.ExecuteNonQuery(out strReturn, "@Msg");
                return strReturn;
            }
            catch (Exception Ex)
            {
                return "Failed" + Ex.Message;
                throw;
            }
        }

        public DataTable GetProviderMOUWithInsurerSpecific(Int32 ProviderID, int UserID, Int16 TariffType = 0, Int16 MouStatus = 0) //SP3V-2777 Leena Added Tariff Type Parameter
        {
            try
            {
                DataTable dt = new DataTable();
                //select * from ProviderMOU where ProviderID=@ProviderID and Deleted=0  Order BY EndDate Desc;
                //vDBHelper.mCreateCommand(CommandType.Text, "select *,case when datediff(day,convert(date,getdate()),convert(date,EndDate))>=0 then 'Edit' else '' end as Active from ProviderMOU where ProviderID=@ProviderID and Deleted=0");
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "USP_ProviderMOU_DetailsInsurerSpecific");
                vDBHelper.mAddParameter("@ProviderID", SqlDbType.BigInt, ParameterDirection.Input, ProviderID);
                vDBHelper.mAddParameter("@TariffType", SqlDbType.TinyInt, ParameterDirection.Input, TariffType);
                vDBHelper.mAddParameter("@MouStatus", SqlDbType.TinyInt, ParameterDirection.Input, MouStatus);
                vDBHelper.mAddParameter("@UserID", SqlDbType.Int, ParameterDirection.Input, UserID);
                dt = vDBHelper.ExecuteDT();
                return dt;
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
        }


        public DataTable GetProviderMOU(Int32 ProviderID, Int16 TariffType = 0, Int16 MouStatus = 0) //SP3V-2777 Leena Added Tariff Type Parameter
        {
            try
            {
                DataTable dt = new DataTable();
                //select * from ProviderMOU where ProviderID=@ProviderID and Deleted=0  Order BY EndDate Desc;
                //vDBHelper.mCreateCommand(CommandType.Text, "select *,case when datediff(day,convert(date,getdate()),convert(date,EndDate))>=0 then 'Edit' else '' end as Active from ProviderMOU where ProviderID=@ProviderID and Deleted=0");
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "USP_ProviderMOU_Details");
                vDBHelper.mAddParameter("@ProviderID", SqlDbType.BigInt, ParameterDirection.Input, ProviderID);
                vDBHelper.mAddParameter("@TariffType", SqlDbType.TinyInt, ParameterDirection.Input, TariffType); //SP3V-2777 Leena Added Tariff Type Parameter
                vDBHelper.mAddParameter("@MouStatus", SqlDbType.TinyInt, ParameterDirection.Input, MouStatus); //SP3V-2777 Leena Added Tariff Type Parameter
                dt = vDBHelper.ExecuteDT();
                return dt;
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
        }

        #endregion

        #region Insert and Update TDS
        public DataTable GetProviderReqTDS(string ProvReqID)
        {
            try
            {
                string tdsQuery = "SELECT ID, ProviderReqID, TANNo, CertificateNo, TaxExcemtionFrom, Excemption, ThresholdPerc, ThresholdAmt, TDSPerc, TDSAmount, isMin, DMSID, ISSUEID FROM ProviderReqTDS WHERE Deleted = 0 AND ProviderReqID = @ProvReqID";
                vDBHelper.mCreateCommand(CommandType.Text, tdsQuery);
                vDBHelper.mAddParameter("@ProvReqID", SqlDbType.BigInt, ParameterDirection.Input, ProvReqID);
                DataTable dt = vDBHelper.ExecuteDT();
                return dt;
            }
            catch (Exception Ex)
            {
                throw;
            }
        }

        public string UpdateTDS(TDSDetails tds)
        {
            try
            {
                DateTime d;
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "Usp_ProvReqTDS_Insert");
                vDBHelper.mAddParameter("@ProviderReqID", SqlDbType.BigInt, ParameterDirection.Input, tds.ProviderReqID);
                vDBHelper.mAddParameter("@ID", SqlDbType.BigInt, ParameterDirection.Input, tds.ID);
                vDBHelper.mAddParameter("@TANNo", SqlDbType.VarChar, ParameterDirection.Input, tds.TANNo);
                vDBHelper.mAddParameter("@CertificateNo", SqlDbType.VarChar, ParameterDirection.Input, tds.CertificateNo);
                if (DateTime.TryParse(tds.TaxExcemtionFrom, out d))
                    vDBHelper.mAddParameter("@TaxExcemtionFrom", SqlDbType.DateTime, ParameterDirection.Input, tds.TaxExcemtionFrom);
                vDBHelper.mAddParameter("@Excemption", SqlDbType.Decimal, ParameterDirection.Input, tds.Excemption);
                vDBHelper.mAddParameter("@ThresholdPerc", SqlDbType.Decimal, ParameterDirection.Input, tds.ThresholdPerc);
                vDBHelper.mAddParameter("@ThresholdAmt", SqlDbType.Decimal, ParameterDirection.Input, tds.ThresholdAmt);
                vDBHelper.mAddParameter("@TDSPerc", SqlDbType.Decimal, ParameterDirection.Input, tds.TDSPerc);
                vDBHelper.mAddParameter("@TDSAmount", SqlDbType.Decimal, ParameterDirection.Input, tds.TDSAmount);
                vDBHelper.mAddParameter("@EffectiveFrom", SqlDbType.DateTime, ParameterDirection.Input, System.DateTime.Now);
                vDBHelper.mAddParameter("@Remarks", SqlDbType.VarChar, ParameterDirection.Input, tds.Remarks);
                vDBHelper.mAddParameter("@DMSID", SqlDbType.Int, ParameterDirection.Input, tds.DMSID);
                vDBHelper.mAddParameter("@CreatedUserRegionID", SqlDbType.Int, ParameterDirection.Input, tds.CreatedUserRegionID);
                vDBHelper.mAddParameter("@ISSUEID", SqlDbType.TinyInt, ParameterDirection.Input, tds.ISSUEID);
                vDBHelper.mAddParameter("@Msg", SqlDbType.VarChar, 1000, ParameterDirection.Output, "");
                vDBHelper.mAddParameter("@EffectiveTo", SqlDbType.DateTime, ParameterDirection.Input, tds.EffectiveTo);
                vDBHelper.mAddParameter("@FinYearStartDate", SqlDbType.DateTime, ParameterDirection.Input, tds.FinYearStartDate);
                vDBHelper.mAddParameter("@FinYearEndDate", SqlDbType.DateTime, ParameterDirection.Input, tds.FinYearEndDate);
                //vDBHelper.mAddParameter("@TaxExcemtionTo", SqlDbType.DateTime, ParameterDirection.Input, tds.TaxExcemtionTo);   
                string strReturn;
                vDBHelper.ExecuteNonQuery(out strReturn, "@Msg");
                return strReturn;
            }
            catch (Exception Ex)
            {
                return "Failed" + Ex.Message;
                throw;
            }
        }

        public string UpdateProviderTDS(ProviderTDSDetails tds)
        {
            try
            {
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "Usp_ProviderTDS_Insert");
                vDBHelper.mAddParameter("@ProviderID", SqlDbType.BigInt, ParameterDirection.Input, tds.ProviderID);
                vDBHelper.mAddParameter("@MOUID", SqlDbType.Int, ParameterDirection.Input, DBNull.Value);
                vDBHelper.mAddParameter("@TANNo", SqlDbType.VarChar, ParameterDirection.Input, tds.TANNo);
                vDBHelper.mAddParameter("@CertificateNo", SqlDbType.VarChar, ParameterDirection.Input, tds.CertificateNo);
                //if (tds.TaxExcemtionFrom != "" && tds.TaxExcemtionFrom != null)
                //    vDBHelper.mAddParameter("@TaxExcemtionFrom", SqlDbType.DateTime, ParameterDirection.Input, tds.TaxExcemtionFrom);
                //vDBHelper.mAddParameter("@Excemption", SqlDbType.Decimal, ParameterDirection.Input, tds.Excemption);
                vDBHelper.mAddParameter("@ThresholdPerc", SqlDbType.Decimal, ParameterDirection.Input, tds.ThresholdPerc);
                vDBHelper.mAddParameter("@ThresholdAmt", SqlDbType.Decimal, ParameterDirection.Input, tds.ThresholdAmt);
                vDBHelper.mAddParameter("@TDSPerc", SqlDbType.Decimal, ParameterDirection.Input, tds.TDSPerc);
                vDBHelper.mAddParameter("@TDSAmount", SqlDbType.Decimal, ParameterDirection.Input, tds.TDSAmount);
                vDBHelper.mAddParameter("@EffectiveFrom", SqlDbType.DateTime, ParameterDirection.Input, tds.EffectiveFrom);
                vDBHelper.mAddParameter("@Remarks", SqlDbType.VarChar, ParameterDirection.Input, tds.Remarks);
                vDBHelper.mAddParameter("@DMSID", SqlDbType.Int, ParameterDirection.Input, tds.DMSID);
                vDBHelper.mAddParameter("@CreatedUserRegionID", SqlDbType.Int, ParameterDirection.Input, tds.CreatedUserRegionID);
                vDBHelper.mAddParameter("@IssueID", SqlDbType.Int, ParameterDirection.Input, tds.ISSUEID);
                vDBHelper.mAddParameter("@Msg", SqlDbType.VarChar, 1000, ParameterDirection.Output, "");
                vDBHelper.mAddParameter("@EffectiveTo", SqlDbType.DateTime, ParameterDirection.Input, tds.EffectiveTo);
                vDBHelper.mAddParameter("@FinYearStartDate", SqlDbType.DateTime, ParameterDirection.Input, tds.FinYearStartDate);
                vDBHelper.mAddParameter("@FinYearEndDate", SqlDbType.DateTime, ParameterDirection.Input, tds.FinYearEndDate);
                // vDBHelper.mAddParameter("@TaxExcemtionTo", SqlDbType.DateTime, ParameterDirection.Input, tds.TaxExcemtionTo); 
                string strReturn;
                vDBHelper.ExecuteNonQuery(out strReturn, "@Msg");
                return strReturn;
            }
            catch (Exception Ex)
            {
                return "Failed" + Ex.Message;
                throw;
            }
        }

        public DataTable GetProviderTDS(Int32 ProviderID, Int16 Type)
        {
            try
            {
                DataTable dt = new DataTable();
                if (Type == 1)
                {
                    vDBHelper.mCreateCommand(CommandType.Text, "select ISSUEID,FinYearStartDate,FinYearEndDate,EffectiveFrom,EffectiveTo,TANNo,CertificateNo,TDSPerc,TDSAmount,Createddatetime,CreatedUserRegionID,Remarks,ID,ProviderID,MOUID,ThresholdPerc,ThresholdAmt from ProviderTDS where ProviderID=@ProviderID  and FinYearEndDate > DATEADD(year,-2,GETDATE())  and Deleted=0  Order By Createddatetime DESC");
                }
                else
                {
                    vDBHelper.mCreateCommand(CommandType.Text, "select ISSUEID,FinYearStartDate,FinYearEndDate,EffectiveFrom,EffectiveTo,TANNo,CertificateNo,TDSPerc,TDSAmount,Createddatetime,CreatedUserRegionID,Remarks,ID,ProviderID,MOUID,ThresholdPerc,ThresholdAmt from ProviderTDS where ProviderID=@ProviderID   and Deleted=0  Order By Createddatetime DESC");
                }
                vDBHelper.mAddParameter("@ProviderID", SqlDbType.BigInt, ParameterDirection.Input, ProviderID);
                //vDBHelper.mAddParameter("@MOUID", SqlDbType.BigInt, ParameterDirection.Input, MOUID);
                dt = vDBHelper.ExecuteDT();
                return dt;
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
        }
        #endregion

        #region Insert and Update BankDetails
        public string UpdateBankDetails(BankDetails bank, string Deducteename, string IRDACode)
        {
            try
            {
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "Usp_ProvReqBank_Insert");
                vDBHelper.mAddParameter("@ProviderReqID", SqlDbType.BigInt, ParameterDirection.Input, bank.ProviderReqID);
                vDBHelper.mAddParameter("@IFSCCode", SqlDbType.VarChar, ParameterDirection.Input, bank.IFSCCode);
                vDBHelper.mAddParameter("@BankName", SqlDbType.VarChar, ParameterDirection.Input, bank.BankName);
                vDBHelper.mAddParameter("@BranchName", SqlDbType.VarChar, ParameterDirection.Input, bank.BranchName);
                vDBHelper.mAddParameter("@MICRCode", SqlDbType.VarChar, ParameterDirection.Input, bank.MICRCode);
                vDBHelper.mAddParameter("@Accountnumber", SqlDbType.VarChar, ParameterDirection.Input, bank.Accountnumber);
                vDBHelper.mAddParameter("@Accounttypeid", SqlDbType.Int, ParameterDirection.Input, bank.Accounttypeid);
                vDBHelper.mAddParameter("@payeename", SqlDbType.VarChar, ParameterDirection.Input, bank.payeename);
                if (bank.EffectiveDate != null && bank.EffectiveDate != "")
                    vDBHelper.mAddParameter("@EffectiveDate", SqlDbType.DateTime, ParameterDirection.Input, Convert.ToDateTime(bank.EffectiveDate));
                vDBHelper.mAddParameter("@Remarks", SqlDbType.VarChar, ParameterDirection.Input, bank.Remarks);
                vDBHelper.mAddParameter("@DMSID", SqlDbType.Int, ParameterDirection.Input, bank.DMSID);
                vDBHelper.mAddParameter("@CreatedUserRegionID", SqlDbType.Int, ParameterDirection.Input, bank.CreatedUserRegionID);
                vDBHelper.mAddParameter("@PanNumber", SqlDbType.VarChar, ParameterDirection.Input, bank.PanNumber);
                vDBHelper.mAddParameter("@Deducteename", SqlDbType.VarChar, ParameterDirection.Input, Deducteename);
                vDBHelper.mAddParameter("@IRDACode", SqlDbType.VarChar, ParameterDirection.Input, IRDACode);
                vDBHelper.mAddParameter("@Msg", SqlDbType.VarChar, 1000, ParameterDirection.Output, "");
                string strReturn;
                vDBHelper.ExecuteNonQuery(out strReturn, "@Msg");
                return strReturn;
            }
            catch (Exception Ex)
            {
                // return "Failed" + Ex.Message;
                throw;
            }
        }

        #region UpdateProviderBankDetails(Changes done for Task: SP-977)
        public string UpdateProviderBankDetails(ProviderBankDetails bank)
        {
            string responseMsg = string.Empty;
            try
            {
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "Usp_ProviderBank_Insert");
                vDBHelper.mAddParameter("@ProviderID", SqlDbType.BigInt, ParameterDirection.Input, bank.ProviderID);
                vDBHelper.mAddParameter("@BankID1", SqlDbType.Int, ParameterDirection.Input, bank.BankID);
                vDBHelper.mAddParameter("@IFSCCode", SqlDbType.VarChar, ParameterDirection.Input, bank.IFSCCode ?? "");
                vDBHelper.mAddParameter("@BankName", SqlDbType.VarChar, ParameterDirection.Input, bank.BankName ?? "");
                vDBHelper.mAddParameter("@BranchName", SqlDbType.VarChar, ParameterDirection.Input, bank.BranchName ?? "");
                vDBHelper.mAddParameter("@MICRCode", SqlDbType.VarChar, ParameterDirection.Input, bank.MICRCode ?? "");
                vDBHelper.mAddParameter("@Accountnumber", SqlDbType.VarChar, ParameterDirection.Input, bank.Accountnumber ?? "");
                vDBHelper.mAddParameter("@Accounttypeid", SqlDbType.Int, ParameterDirection.Input, bank.Accounttypeid);
                vDBHelper.mAddParameter("@payeename", SqlDbType.VarChar, ParameterDirection.Input, bank.payeename ?? "");
                if (bank.EffectiveDate != null && bank.EffectiveDate != "")
                    vDBHelper.mAddParameter("@EffectiveDate", SqlDbType.DateTime, ParameterDirection.Input, Convert.ToDateTime(bank.EffectiveDate));
                vDBHelper.mAddParameter("@Remarks", SqlDbType.VarChar, ParameterDirection.Input, bank.Remarks);
                vDBHelper.mAddParameter("@DMSID", SqlDbType.Int, ParameterDirection.Input, bank.DMSID);
                vDBHelper.mAddParameter("@CreatedUserRegionID", SqlDbType.Int, ParameterDirection.Input, bank.CreatedUserRegionID);
                //vDBHelper.mAddParameter("@PanNumber", SqlDbType.VarChar, ParameterDirection.Input, bank.PanNumber);
                if (!string.IsNullOrEmpty(bank.Emailid))
                    vDBHelper.mAddParameter("@Emailid", SqlDbType.VarChar, ParameterDirection.Input, bank.Emailid);
                if (!string.IsNullOrEmpty(bank.SecEmailID))
                    vDBHelper.mAddParameter("@SecEmailID", SqlDbType.VarChar, ParameterDirection.Input, bank.SecEmailID);
                if (bank.MobileNo > 0)
                    vDBHelper.mAddParameter("@MobileNo", SqlDbType.BigInt, ParameterDirection.Input, bank.MobileNo);
                if (bank.AltMobileNo > 0)
                    vDBHelper.mAddParameter("@AltMobileNo", SqlDbType.BigInt, ParameterDirection.Input, bank.AltMobileNo);
                if (!string.IsNullOrEmpty(bank.IssueIds))
                    vDBHelper.mAddParameter("@IssueId", SqlDbType.VarChar, ParameterDirection.Input, bank.IssueIds);

                vDBHelper.mAddParameter("@ApprovalStatus", SqlDbType.TinyInt, ParameterDirection.Input, bank.ApprovalStatus);
                vDBHelper.mAddParameter("@CancelledCheque", SqlDbType.VarChar, ParameterDirection.Input, bank.CancelledCheque);
                vDBHelper.mAddParameter("@PANCopy", SqlDbType.VarChar, ParameterDirection.Input, bank.PANCopy);
                vDBHelper.mAddParameter("@Msg", SqlDbType.VarChar, 1000, ParameterDirection.Output, "");

                vDBHelper.ExecuteNonQuery(out responseMsg, "@Msg");
                return responseMsg;
            }
            catch (Exception Ex)
            {
                string msg = Ex.Message;
                // return "Failed" + Ex.Message;
                throw;
            }
        }
        #endregion

        //public string UpdateProviderBankDetails(DataTable dt, int UserRegionID)
        //{
        //    try
        //    {
        //        vDBHelper.mCreateCommand(CommandType.StoredProcedure, "Usp_ProviderInsurerBank_Insert");
        //        vDBHelper.mAddParameter("@ProviderInsurerBank", SqlDbType.Structured, ParameterDirection.Input, dt);
        //        vDBHelper.mAddParameter("@CreatedUserRegionID", SqlDbType.Int, ParameterDirection.Input, UserRegionID);
        //        vDBHelper.mAddParameter("@Msg", SqlDbType.VarChar, 1000, ParameterDirection.Output, "");
        //        string strReturn;
        //        vDBHelper.ExecuteNonQuery(out strReturn, "@Msg");
        //        return strReturn;
        //    }
        //    catch (Exception Ex)
        //    {              
        //        throw;
        //    }
        //}
        //    public DataTable GetProviderBankDetails(Int32 ProviderID)
        //    { 
        //        try
        //        {
        //            DataTable dt = new DataTable();
        //            vDBHelper.mCreateCommand(CommandType.Text, "select ID,ProviderID,B.Name BankName,B.Branch,B.IFSCCode,B.MICRCode,PB.AccountNumber,AccountTypeID,Payeename,EffectiveDate,Emailid,SecEmailID "
        //+ " from ProviderBankDetails PB,MSt_Bank B where PB.BankID=B.ID and PB.Deleted=0 and PB.ProviderID=@ProviderID");
        //            vDBHelper.mAddParameter("@ProviderID", SqlDbType.BigInt, ParameterDirection.Input, ProviderID);
        //            dt = vDBHelper.ExecuteDT();
        //            return dt;
        //        }
        //        catch (Exception Ex)
        //        {
        //            throw Ex;
        //        }
        //    }

        #endregion

        #region Insert and Update Early Payments
        public string UpdateEarlyPayment(List<EarlyPayment> early_Payments)
        {
            try
            {
                string strReturn = "";
                foreach (EarlyPayment pay in early_Payments)
                {
                    vDBHelper.mCreateCommand(CommandType.StoredProcedure, "Usp_ProvReqEarlyPayDisc_Insert");
                    vDBHelper.mAddParameter("@ProviderReqID", SqlDbType.BigInt, ParameterDirection.Input, pay.ProviderReqID);
                    vDBHelper.mAddParameter("@ID", SqlDbType.Int, ParameterDirection.Input, pay.ID);
                    vDBHelper.mAddParameter("@NoofDays", SqlDbType.Int, ParameterDirection.Input, pay.NoofDays);
                    vDBHelper.mAddParameter("@Percentage", SqlDbType.Decimal, ParameterDirection.Input, pay.Percentage);
                    vDBHelper.mAddParameter("@CreatedUserRegionID", SqlDbType.Int, ParameterDirection.Input, pay.CreatedUserRegionID);
                    vDBHelper.mAddParameter("@Msg", SqlDbType.VarChar, 1000, ParameterDirection.Output, "");
                    vDBHelper.ExecuteNonQuery(out strReturn, "@Msg");
                    if (strReturn == "")
                    {
                        strReturn = "Early Payment Details Saved";
                    }
                }
                return strReturn;
            }
            catch (Exception Ex)
            {
                //return "Failed" + Ex.Message;
                throw;
            }
        }

        public string pay()
        {
            return "Success";
        }
        #endregion

        #region FinalConfirm For Hospital Login
        public DataSet VerifyMOU(VerifyMOU mou, string CommunicationRemarks, long DMSID, out string strReturn)
        {
            try
            {
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "Usp_ProvReqMOUFinal");
                vDBHelper.mAddParameter("@ProviderReqID", SqlDbType.BigInt, ParameterDirection.Input, mou.ProviderReqID);
                vDBHelper.mAddParameter("@CreatedUserRegionID", SqlDbType.Int, ParameterDirection.Input, mou.CreatedUserRegionID);
                vDBHelper.mAddParameter("@MOUTypeID", SqlDbType.Int, ParameterDirection.Input, mou.MOUTypeID);
                vDBHelper.mAddParameter("@MOUEntityID", SqlDbType.Int, ParameterDirection.Input, mou.MOUEntityID);
                vDBHelper.mAddParameter("@DMSID", SqlDbType.BigInt, ParameterDirection.Input, DMSID);
                vDBHelper.mAddParameter("@CommunicationRemarks", SqlDbType.VarChar, ParameterDirection.Input, CommunicationRemarks);
                vDBHelper.mAddParameter("@Msg", SqlDbType.VarChar, 1000, ParameterDirection.Output, "");

                DataSet ds = vDBHelper.ExecutewithSingleparam(out strReturn, "@Msg");
                return ds;
            }
            catch (Exception Ex)
            {
                //return "Failed" + Ex.Message;
                throw;
            }
        }
        #endregion

        #region Final Verification For FHPL Login
        //USP_ProvReqMOU_Verification(@ProviderReqID bigint,@statusid int,@MOUTypeID int,@MOUEntityID bigint,@EffectiveFrom Datetime,@EffectiveTo Datetime,
        //@CreatedUserRegionID int,@Remarks varchar(500),@Msg varchar(10pa00) OUTPUT)
        public DataSet FinalMOU(ConfirmMou mou, string CommunicationRemarks, long DMSID, out string strReturn)
        {
            try
            {
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "USP_ProvReqMOU_VerificationandEmpanellement");
                vDBHelper.mAddParameter("@ProviderReqID", SqlDbType.BigInt, ParameterDirection.Input, mou.ProviderReqID);
                vDBHelper.mAddParameter("@statusid", SqlDbType.Int, ParameterDirection.Input, mou.statusid);
                vDBHelper.mAddParameter("@MOUTypeID", SqlDbType.Int, ParameterDirection.Input, mou.MOUTypeID);
                vDBHelper.mAddParameter("@MOUEntityID", SqlDbType.BigInt, ParameterDirection.Input, mou.MOUEntityID);
                if (mou.EffectiveFrom != "")
                {
                    vDBHelper.mAddParameter("@EffectiveFrom", SqlDbType.DateTime, ParameterDirection.Input, Convert.ToDateTime(mou.EffectiveFrom));
                }
                if (mou.EffectiveTo != "")
                {
                    vDBHelper.mAddParameter("@EffectiveTo", SqlDbType.DateTime, ParameterDirection.Input, Convert.ToDateTime(mou.EffectiveTo));
                }
                vDBHelper.mAddParameter("@CreatedUserRegionID", SqlDbType.Int, ParameterDirection.Input, mou.CreatedUserRegionID);
                vDBHelper.mAddParameter("@Remarks", SqlDbType.VarChar, ParameterDirection.Input, mou.Remarks);
                vDBHelper.mAddParameter("@DMSID", SqlDbType.BigInt, ParameterDirection.Input, DMSID);
                vDBHelper.mAddParameter("@CommunicationRemarks", SqlDbType.VarChar, ParameterDirection.Input, CommunicationRemarks);
                vDBHelper.mAddParameter("@Msg", SqlDbType.VarChar, 1000, ParameterDirection.Output, "");

                DataSet ds = vDBHelper.ExecutewithSingleparam(out strReturn, "@Msg");
                return ds;
            }
            catch (Exception Ex)
            {
                //return "Failed" + Ex.Message;
                throw;
            }
        }
        #endregion

        #region Facilities
        public string PostFacilities(Facility f)
        {
            try
            {
                //Usp_ProvReqFacility_Modify(@ProvReqID bigint,@ProvReqFacilities ProviderFacilities ReadOnly, @CreatedUserRegionID int,@Msg varchar(1000) OUTPUT)
                DataTable dt = new DataTable();
                dt.Columns.Add("FacilityID", typeof(int));
                dt.Columns.Add("DisplayName", typeof(string));
                dt.Columns.Add("FacilityValue", typeof(string));
                dt.Columns.Add("DMSID", typeof(int));
                dt.Columns.Add("Percentage", typeof(decimal));

                foreach (RoomTypes rtype in f.types)
                {
                    dt.Rows.Add(rtype.FacilityID, rtype.DisplayName, rtype.FacilityValue, rtype.DMSID, rtype.Percentage);
                }

                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "Usp_ProvReqFacility_Modify");
                vDBHelper.mAddParameter("@ProvReqID", SqlDbType.BigInt, ParameterDirection.Input, f.ProvReqID);
                vDBHelper.mAddParameter("@CreatedUserRegionID", SqlDbType.Int, ParameterDirection.Input, f.CreatedUserRegionID);
                vDBHelper.mAddParameter("@ProvReqFacilities", SqlDbType.Structured, ParameterDirection.Input, dt);
                vDBHelper.mAddParameter("@Msg", SqlDbType.VarChar, 1000, ParameterDirection.Output, "");
                string strReturn;
                vDBHelper.ExecuteNonQuery(out strReturn, "@Msg");
                return strReturn;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public string FacilityOptions()
        {
            return "Success";
        }
        #endregion

        #region DashBoard

        #region LoadDashBoard
        public DataSet LoadDashBoard(string RoleID, string RegionID, int? LoginUserID = null)
        {
            try
            {
                if (RoleID == "28")
                {
                    vDBHelper.mCreateCommand(CommandType.StoredProcedure, "USP_PROVHOSPDASHBOARD");
                    vDBHelper.mAddParameter("@RoleID", SqlDbType.Int, ParameterDirection.Input, RoleID);
                    vDBHelper.mAddParameter("@ProvReqID", SqlDbType.BigInt, ParameterDirection.Input, RegionID);
                    vDBHelper.mAddParameter("@LoginUserID", SqlDbType.Int, ParameterDirection.Input, LoginUserID);
                    DataSet ds = vDBHelper.Execute();
                    return ds;
                }
                else
                {

                    vDBHelper.mCreateCommand(CommandType.StoredProcedure, "USP_PROVDASHBOARD");
                    vDBHelper.mAddParameter("@RoleID", SqlDbType.Int, ParameterDirection.Input, RoleID);
                    vDBHelper.mAddParameter("@RegionID", SqlDbType.Int, ParameterDirection.Input, RegionID);
                    vDBHelper.mAddParameter("@LoginUserID", SqlDbType.Int, ParameterDirection.Input, LoginUserID);
                    DataSet ds = vDBHelper.Execute();
                    return ds;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #endregion

        #region SearchDashBoard
        public DataSet SearchDashBoard(string ROLEID, string RegionID, string stageID, string Todo, string tatexceed, string total, string NearTAT, string ReqID, string Name,
                                      string Location, string StateID, string CityID, string FromDate, string ToDate, string PRCNo)
        {
            try
            {
                DateTime d;
                long l;
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "USP_PROVREQSEARCH");
                vDBHelper.mAddParameter("@ROLEID", SqlDbType.Int, ParameterDirection.Input, ROLEID);
                vDBHelper.mAddParameter("@RegionID", SqlDbType.Int, ParameterDirection.Input, RegionID);
                if (stageID != "0")
                    vDBHelper.mAddParameter("@stageID", SqlDbType.Int, ParameterDirection.Input, stageID);
                vDBHelper.mAddParameter("@todo", SqlDbType.Bit, ParameterDirection.Input, Todo);
                vDBHelper.mAddParameter("@tatexceed", SqlDbType.Bit, ParameterDirection.Input, tatexceed);
                vDBHelper.mAddParameter("@total", SqlDbType.Bit, ParameterDirection.Input, total);
                vDBHelper.mAddParameter("@NearTAT", SqlDbType.Bit, ParameterDirection.Input, NearTAT);
                if (ReqID != "0")
                    vDBHelper.mAddParameter("@ReqID", SqlDbType.BigInt, ParameterDirection.Input, ReqID);
                if (Name != "null" && Name != null)
                    vDBHelper.mAddParameter("@Name", SqlDbType.VarChar, ParameterDirection.Input, Name);

                if (Location != "null" && Location != null)
                    vDBHelper.mAddParameter("@Location", SqlDbType.VarChar, ParameterDirection.Input, Location);
                if (StateID != "0" && StateID != "")
                    vDBHelper.mAddParameter("@StateID", SqlDbType.Int, ParameterDirection.Input, StateID);
                if (CityID != "0")// && CityID!=null)
                    vDBHelper.mAddParameter("@CityID", SqlDbType.Int, ParameterDirection.Input, CityID);

                if (DateTime.TryParse(FromDate, out d))
                    vDBHelper.mAddParameter("@FromDate", SqlDbType.DateTime, ParameterDirection.Input, FromDate);
                if (DateTime.TryParse(ToDate, out d))
                    vDBHelper.mAddParameter("@ToDate", SqlDbType.DateTime, ParameterDirection.Input, ToDate);

                if (PRCNo != "null" && PRCNo != null)
                    vDBHelper.mAddParameter("@PRCNo", SqlDbType.VarChar, ParameterDirection.Input, PRCNo);

                DataSet ds = vDBHelper.Execute();
                return ds;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #endregion

        #endregion

        #region Login
        public DataSet Login(Login login)
        {
            try
            {
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "Usp_Provider_Login");
                vDBHelper.mAddParameter("@UserName", SqlDbType.VarChar, ParameterDirection.Input, login.UserName);
                vDBHelper.mAddParameter("@Password", SqlDbType.VarChar, ParameterDirection.Input, login.Password);
                vDBHelper.mAddParameter("@Type", SqlDbType.TinyInt, ParameterDirection.Input, login.Type);

                DataSet ds = vDBHelper.Execute();
                return ds;

            }
            catch (Exception Ex)
            {
                throw;
            }
        }

        public string LoginProcess()
        {
            return "Success";
        }
        #endregion

        #region Reports
        public DataTable AgeingReport()
        {
            try
            {
                //                [
                //data : {
                //   'age':['0-18', '19-25', '26-35', '36-45'],
                //   'NoOfClaimsRecd': [39, 200, 400, 250],
                //   'NoofSettled': [35, 150, 300, 200],
                //  'ofSettledClaims': [90, 80, 75, 78]
                //},
                //// this optional
                //displayname: {
                //   'NoOfClaimsRecd': "No Of Claims Recd",
                // NoofSettled : 'No. of Settled',
                // ofSettledClaims : '% of Settled Claims'
                //}

                //]
                // DataSet ds = new DataSet();
                DataTable dt = new DataTable();
                dt.Columns.Add("age");
                dt.Columns.Add("NoOfClaimsRecd");
                dt.Columns.Add("NoofSettled");
                dt.Columns.Add("ofSettledClaims");
                dt.Rows.Add("'0-18','19-25','26-35','36-45'", "39, 200, 400, 250", "35, 150, 300, 200", "90, 80, 75, 78");

                return dt;
            }
            catch (Exception Ex)
            {
                throw;
            }
            finally
            {
                //if (sCon.State == ConnectionState.Open)
                //    sCon.Close();
            }
        }
        #endregion
        #region nonnetworks
        public DataSet GetProviderDetails(string Name, int CityID, string Location, string PINCode, string RegMobileNo, int? ProviderID, int? StateID = 0, int? DistrictID = 0)
        {
            try
            {
                Location = Location.Replace("'", "''''");
                Name = Name.Replace("'", "''''");
                DataSet ds;
                // vDBHelper.mCreateCommand(CommandType.Text, "SELECT p.Name ,p.CountryID,p.StateID,p.DistrictID,p.CityID,p.Location,p.PINCode,p.countryname,p.statename,p.districname,p.cityname,p.RegLandlineNo FROM VW_GetAllProviderDetails p where   p.Name='" + Name + "' and (CityID=" + CityID + " and Location like '%" + Location + "%' and  PINCode='" + PINCode + "' and RIGHT(RegLandlineNo,10)=RIGHT('" + RegMobileNo + "',10) and ID!=" + ProviderID);
                string Query = "SELECT p.Name ,case when p.status =23  then 'Open'  when p.status='24' then  'Rejected' when p.status=25 then 'Accepted' else ''  end as Status, p.CountryID,p.StateID,p.DistrictID,p.CityID,p.Location,p.PINCode,p.countryname,p.statename,p.districname,p.cityname,isnull(p.STDCode,'') as STDCode,isnull(p.RegLandlineNo,'') as RegLandlineNo  FROM VW_GetAllProviderDetails p where   p.Name='" + Name + "'  and  Location like '%" + Location + "%' and  PINCode='" + PINCode + "' and (  CityID=" + CityID + " or   stateid = " + StateID + " or districtid =" + DistrictID + " ) and  ID!=" + ProviderID;

                vDBHelper.mCreateCommand(CommandType.Text, Query);
                ds = vDBHelper.Execute();

                //vDBHelper.mCreateCommand(CommandType.StoredProcedure, "GetProviderDetails");
                //vDBHelper.mAddParameter("@Name", SqlDbType.VarChar, ParameterDirection.Input, Name);
                //vDBHelper.mAddParameter("@CityID", SqlDbType.Int, ParameterDirection.Input, CityID);
                //vDBHelper.mAddParameter("@Location", SqlDbType.VarChar, ParameterDirection.Input, Location);
                //vDBHelper.mAddParameter("@PINCode", SqlDbType.BigInt, ParameterDirection.Input, PINCode);
                //DataSet ds = vDBHelper.Execute();

                return ds;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public DataSet GetCreatedByUserIDByProviderID(int providerID)
        {
            try
            {
                DataSet ds;
                vDBHelper.mCreateCommand(CommandType.Text, "SELECT * FROM Provider where  ID=" + providerID);
                ds = vDBHelper.Execute();
                return ds;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataSet GetNonNetworkHospitalDetailsByProviderID(int? ProviderID)
        {
            try
            {
                DataSet ds;
                vDBHelper.mCreateCommand(CommandType.Text, "SELECT * FROM Provider where  ID=" + ProviderID);
                ds = vDBHelper.Execute();
                return ds;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int CreateProviderMouDates(int ProviderMOUID, DateTime StartDate, DateTime EndDate, int UserRegionID)
        {
            try
            {
                DateTime d;
                int i = 0;

                string Query = "insert into providerMOU (ProviderID,MOUTypeID_P44,MOUEntityID,PRCNo,MOUNo,StartDate,EndDate,ExpiryDate,MOUDiscountPerc,MOUDiscountAbs,MOUReceivedDate,EffectiveDate,DMSID,TariffDMSID,PackageDMSID,isLatest,Deleted,Createddatetime,CreatedUserRegionID)";
                Query += "  (select top 1 ProviderID,MOUTypeID_P44,MOUEntityID,PRCNo,MOUNo,convert(date,'" + Convert.ToDateTime(StartDate).ToString("MM/dd/yyyy") + "',101),convert(date,'" + Convert.ToDateTime(EndDate).ToString("MM/dd/yyyy") + "',101),convert(date,'" + Convert.ToDateTime(EndDate).ToString("MM/dd/yyyy") + "',101),MOUDiscountPerc,MOUDiscountAbs,MOUReceivedDate,EffectiveDate,DMSID,TariffDMSID,PackageDMSID,1,0,convert(date,'" + System.DateTime.Now.ToString("MM/dd/yyyy") + "',101)," + UserRegionID + " from providerMOU where id=" + ProviderMOUID + ");SELECT CAST(scope_identity() AS int)";

                vDBHelper.mCreateCommand(CommandType.Text, Query);
                int ID = Convert.ToInt32(vDBHelper.ExecuteScalar());
                if (ID > 0)
                {
                    Query = "insert into ProviderTariff (ProviderID,MOUID,Serviceid,FacilityID,Amount,Discount,Inclusions,Exclusions,UnitType,EffectiveDate,DMSID,isLatest,Deleted,Createddatetime,CreatedUserRegionID)";
                    Query += "  (select ProviderID," + ID.ToString() + ",Serviceid,FacilityID,Amount,Discount,Inclusions,Exclusions,UnitType,EffectiveDate,DMSID,isLatest,Deleted,convert(date,'" + System.DateTime.Now.ToString("MM/dd/yyyy") + "',101)," + UserRegionID + " from ProviderTariff where MOUID=" + ProviderMOUID + "and Deleted=0);";

                    vDBHelper.mCreateCommand(CommandType.Text, Query);
                    vDBHelper.ExecuteNonQuery();

                    Query = "insert into ProviderPackage (ProviderID,MOUID,TPAProcID,Inclusions,Exclusions,FacilityID,Amount,Discount,LOS,EffectiveDate,isLatest,Deleted,Createddatetime,CreatedUserRegionID)";
                    Query += "  (select ProviderID," + ID.ToString() + ",TPAProcID,Inclusions,Exclusions,FacilityID,Amount,Discount,LOS,EffectiveDate,isLatest,Deleted,convert(date,'" + System.DateTime.Now.ToString("MM/dd/yyyy") + "',101)," + UserRegionID + " from ProviderPackage where MOUID=" + ProviderMOUID + " and Deleted=0);";

                    vDBHelper.mCreateCommand(CommandType.Text, Query);
                    vDBHelper.ExecuteNonQuery();

                    Query = "insert into Providertds (ProviderID,MOUID,EffectiveFrom,TANNo,CertificateNo,TaxExcemtionFrom,Excemption,ThresholdPerc,ThresholdAmt,TDSPerc,TDSAmount,isMin,Utilised,StatusID,DMSID,isLatest,Deleted,Createddatetime,CreatedUserRegionID)";
                    Query += "  (select ProviderID," + ID.ToString() + ",EffectiveFrom,TANNo,CertificateNo,TaxExcemtionFrom,Excemption,ThresholdPerc,ThresholdAmt,TDSPerc,TDSAmount,isMin,Utilised,StatusID,DMSID,isLatest,Deleted,convert(date,'" + System.DateTime.Now.ToString("MM/dd/yyyy") + "',101)," + UserRegionID + " from Providertds where MOUID=" + ProviderMOUID + " and Deleted=0);";

                    vDBHelper.mCreateCommand(CommandType.Text, Query);
                    vDBHelper.ExecuteNonQuery();
                }
                return ID;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string SaveCityMaster(string Name, int CountryID, int StateID, int DistrictID, int RegionID)
        {
            int i = 0;
            DataSet ds;
            vDBHelper.mCreateCommand(CommandType.Text, "SELECT ID,Name,StateID FROM Mst_City where CountryID=" + CountryID + " and StateID=" + StateID + " and DistrictID=" + DistrictID + " and Name='" + Name + "'");
            ds = vDBHelper.Execute();
            if (ds.Tables[0].Rows.Count == 0)
            {
                string Query = "insert into Mst_City (Name,CountryID ,StateID,DistrictID,FHPLRegionID,Deleted,CreatedDateTime)";
                Query += " values  ('" + Name + "'," + CountryID + "," + StateID + "," + DistrictID + "," + RegionID + ",0,Convert(date,'" + System.DateTime.Now.ToString("MM/dd/yyyy") + "',101))";

                vDBHelper.mCreateCommand(CommandType.Text, Query);
                i = vDBHelper.ExecuteNonQuery();
            }
            else
            {
                i = -1;
            }
            return i.ToString();
        }


        public string SaveNonNetworkingHospitals(string Name, string Address1, string Address2, int CountryID, int StateID, int DistrictID, int CityID, string Location, string PINCode, string RegSTDCode,
          string RegMobileNo, string RegFaxNo, string RegEmailID, string Website, string CityName, int CreatedUserRegionID, int? ProviderType, string MobileNo, string FileName)
        {
            try
            {
                long l;
                if (PINCode == "")
                    PINCode = null;
                if (RegSTDCode == "")
                    RegSTDCode = null;
                if (RegMobileNo == "")
                    RegMobileNo = null;
                if (RegFaxNo == "")
                    RegFaxNo = null;
                if (ProviderType == 0)
                    ProviderType = null;

                Name = Name.Replace("'", "''''");
                Location = Location.Replace("'", "''''");

                Address1 = Address1.Replace("'", "''''");
                Address2 = Address2.Replace("'", "''''");

                string dt = Convert.ToDateTime(System.DateTime.Now).ToString("yyyy-MM-dd");

                string Query = "insert into provider (Name,Address1,Address2,CountryID,StateID, DistrictID,CityID,Location,PINCode";
                if (long.TryParse(RegMobileNo, out l))
                    Query = Query + " ,STDCode,RegLandlineNo";

                if (long.TryParse(MobileNo, out l))
                    Query = Query + " ,RegMobileNo";//, + MobileNo + "'";

                if (long.TryParse(RegFaxNo, out l))
                    Query = Query + " ,RegFaxNo";
                if (ProviderType != null)
                    Query = Query + " ,HospitalType_P37";
                Query = Query + ",RegEmailID,Deleted,Website,status,Createddatetime,CityName,CreatedUserRegionID,IsVerifiedFileName) values ('" + Name + "','" + Address1 + "','" + Address2 + "'," + CountryID + "," + StateID + "," + DistrictID + "," + CityID + ",'" + Location + "','" + PINCode + "',";
                if (long.TryParse(RegMobileNo, out l))
                    Query = Query + "'" + RegSTDCode + "','" + RegMobileNo + "','" + MobileNo + "',";

                if (long.TryParse(RegFaxNo, out l))
                    Query = Query + RegFaxNo + ",";
                if (ProviderType != null)
                    Query = Query + ProviderType + ",";
                Query = Query + "'" + RegEmailID + "',1,'" + Website + "',23,'" + dt + "','" + CityName + "'," + CreatedUserRegionID + ",'" + FileName + "')";

                vDBHelper.mCreateCommand(CommandType.Text, Query);
                int i = vDBHelper.ExecuteNonQuery();

                return i.ToString();


            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public string UpdateNonNetworkingHospitals(int ProviderID, string Name, string Address1, string Address2, int CountryID, int StateID, int DistrictID, int CityID, string Location, string PINCode, string RegSTDCode,
            string RegMobileNo, string RegFaxNo, string RegEmailID, string Website, string CityName, int CreatedUserRegionID, int? ProviderType, string MobileNo, string FileName)
        {
            try
            {
                long l;
                if (PINCode == "")
                    PINCode = null;
                if (RegSTDCode == "")
                    RegSTDCode = null;
                if (RegMobileNo == "")
                    RegMobileNo = null;
                if (RegFaxNo == "")
                    RegFaxNo = null;
                if (ProviderType == 0)
                    ProviderType = null;
                Name = Name.Replace("'", "''''");
                Location = Location.Replace("'", "''''");

                Address1 = Address1.Replace("'", "''''");
                Address2 = Address2.Replace("'", "''''");

                string dt = Convert.ToDateTime(System.DateTime.Now).ToString("yyyy-MM-dd");

                string Query = "update  provider set Name='" + Name + "',Address1='" + Address1 + "',Address2='" + Address2 + "',CountryID=" + CountryID + ",StateID=" + StateID + ", DistrictID=" + DistrictID + ",CityID=" + CityID;
                Query = Query + ",Location='" + Location + "',PINCode='" + PINCode + "'" + ",STDCode = '" + RegSTDCode + "'";
                if (long.TryParse(RegMobileNo, out l))
                    Query = Query + " ,RegLandlineNo='" + RegMobileNo + "'";
                else
                    Query = Query + " ,RegLandlineNo=null";

                if (long.TryParse(MobileNo, out l))
                    Query = Query + " ,RegMobileNo='" + MobileNo + "'";
                else
                    Query = Query + " ,RegMobileNo=null";

                if (long.TryParse(RegFaxNo, out l))
                    Query = Query + " ,RegFaxNo=" + RegFaxNo;
                else
                    Query = Query + " ,RegFaxNo=null";
                //Query = Query + " ,RegEmailID='" + RegEmailID + "',Website='" + Website + " ,CityName='" + CityName + " ,CreatedUserRegionID='" + CreatedUserRegionID + " ,ProviderType='" + ProviderType + "'";
                Query += ", RegEmailID = '" + RegEmailID + "'";
                Query += ", Website = '" + Website + "'";
                Query += ", CityName = '" + CityName + "'";
                Query += ", CreatedUserRegionID = '" + CreatedUserRegionID + "'";
                Query += ", HospitalType_P37 = '" + ProviderType + "'";
                Query += ", IsVerifiedFileName = '" + FileName + "'";
                Query += ", Status=23 ";

                Query = Query + " where ID=" + ProviderID;

                vDBHelper.mCreateCommand(CommandType.Text, Query);
                int i = vDBHelper.ExecuteNonQuery();

                return i.ToString();


            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public string UpdateByProviderId(int ProviderID, string Name, string Address1, string Address2, string Location, string PINCode, string RegEmailID, string MobileNo,
                 int CountryID,
                 int StateID,
                 int DistrictID,
                 int CityID)
        {
            try
            {
                long l;
                if (PINCode == "")
                    PINCode = null;
                Location = Location.Replace("'", "''''");
                Address1 = Address1.Replace("'", "''''");
                var Query = $"update provider set Name  = '{Name}'" +
                    $", Address1 = '{Address1}'" +
                    $", Address2 = '{Address2}'" +
                    $", Location = '{Location}'" +
                    $", PINCode = '{PINCode}'" +
                    $", CountryID = {CountryID}" +
                    $", StateID = {StateID}" +
                    $", DistrictID = {DistrictID}" +
                    $", CityID = {CityID}" +
                    $", RegEmailID = '{RegEmailID}'" +
                    $", RegMobileNo = '{(long.TryParse(MobileNo, out var longMobile) ? longMobile.ToString() : null)}'" +
                    $" where ID = {ProviderID} ";
                vDBHelper.mCreateCommand(CommandType.Text, Query);
                int i = vDBHelper.ExecuteNonQuery();
                return i.ToString();

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }



        public DataSet ValidateProviderUpdate(int ProviderID)
        {
            try
            {
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "Usp_Validate_ProviderUpdate");
                vDBHelper.mAddParameter("@ProviderID", SqlDbType.BigInt, ParameterDirection.Input, ProviderID);
                DataSet ds = vDBHelper.Execute();
                return ds;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public DataSet GetProviderDashBoard(int RoleID, int RegionID)
        {
            try
            {
                if (RoleID == 28)
                    vDBHelper.mCreateCommand(CommandType.StoredProcedure, "");
                else
                    vDBHelper.mCreateCommand(CommandType.StoredProcedure, "USP_PROVDASHBOARD");
                vDBHelper.mAddParameter("@RoleID", SqlDbType.Int, ParameterDirection.Input, RoleID);
                vDBHelper.mAddParameter("@RegionID", SqlDbType.Int, ParameterDirection.Input, RegionID);
                DataSet ds = vDBHelper.Execute();
                return ds;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataSet SearchProviderDashBoard(int ROLEID, int RegionID, int stageID, string Todo, string tatexceed, string total, string NearTAT, int ReqID, string Name,
        string Location, int StateID, int CityID, string FromDate, string ToDate, string PRCNo)
        {
            try
            {
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "USP_PROVREQSEARCH");
                vDBHelper.mAddParameter("@ROLEID", SqlDbType.Int, ParameterDirection.Input, ROLEID);
                vDBHelper.mAddParameter("@RegionID", SqlDbType.Int, ParameterDirection.Input, RegionID);
                if (stageID != 0)
                    vDBHelper.mAddParameter("@stageID", SqlDbType.Int, ParameterDirection.Input, stageID);
                vDBHelper.mAddParameter("@todo", SqlDbType.Bit, ParameterDirection.Input, Todo);
                vDBHelper.mAddParameter("@tatexceed", SqlDbType.Bit, ParameterDirection.Input, tatexceed);
                vDBHelper.mAddParameter("@total", SqlDbType.Bit, ParameterDirection.Input, total);
                vDBHelper.mAddParameter("@NearTAT", SqlDbType.Bit, ParameterDirection.Input, NearTAT);
                if (ReqID != 0)
                    vDBHelper.mAddParameter("@ReqID", SqlDbType.Int, ParameterDirection.Input, ReqID);
                if (Name != "null")
                    vDBHelper.mAddParameter("@Name", SqlDbType.VarChar, ParameterDirection.Input, Name);
                if (Location != "null")
                    vDBHelper.mAddParameter("@Location", SqlDbType.VarChar, ParameterDirection.Input, Location);
                if (StateID != 0)
                    vDBHelper.mAddParameter("@StateID", SqlDbType.Int, ParameterDirection.Input, StateID);
                if (CityID != 0)// && CityID!=null)
                    vDBHelper.mAddParameter("@CityID", SqlDbType.Int, ParameterDirection.Input, CityID);
                if (FromDate != "null")
                    vDBHelper.mAddParameter("@FromDate", SqlDbType.Date, ParameterDirection.Input, FromDate);
                if (ToDate != "null")
                    vDBHelper.mAddParameter("@ToDate", SqlDbType.Date, ParameterDirection.Input, ToDate);
                if (PRCNo != "null" && PRCNo != null)
                    vDBHelper.mAddParameter("@PRCNo", SqlDbType.VarChar, ParameterDirection.Input, PRCNo);
                DataSet ds = vDBHelper.Execute();
                return ds;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataSet SearchNonNetworkProviderDashBoard(int RegionID, string Name, string Location, int StateID, int CityID, string FromDate, string ToDate)
        {
            try
            {
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "USP_NonNetWorkPROVREQSEARCH");
                vDBHelper.mAddParameter("@RegionID", SqlDbType.Int, ParameterDirection.Input, RegionID);
                vDBHelper.mAddParameter("@Name", SqlDbType.VarChar, ParameterDirection.Input, Name);
                if (Location != "null")
                    vDBHelper.mAddParameter("@Location", SqlDbType.VarChar, ParameterDirection.Input, Location);
                if (StateID != 0)
                    vDBHelper.mAddParameter("@StateID", SqlDbType.Int, ParameterDirection.Input, StateID);
                if (CityID != 0)// && CityID!=null)
                    vDBHelper.mAddParameter("@CityID", SqlDbType.Int, ParameterDirection.Input, CityID);
                if (FromDate != "null")
                    vDBHelper.mAddParameter("@FromDate", SqlDbType.Date, ParameterDirection.Input, FromDate);
                if (ToDate != "null")
                    vDBHelper.mAddParameter("@ToDate", SqlDbType.Date, ParameterDirection.Input, ToDate);

                DataSet ds = vDBHelper.Execute();
                return ds;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataSet GetProviderBankDetailsDashboard(string Role, int RegionID, string Name, string Location, int StateID, int CityID, string FromDate, string ToDate)
        {
            try
            {
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "USP_GetProvBankDetailsDashboard");
                vDBHelper.mAddParameter("@Role", SqlDbType.VarChar, ParameterDirection.Input, Role.Trim());
                vDBHelper.mAddParameter("@Name", SqlDbType.VarChar, ParameterDirection.Input, Name.Trim());

                DataSet ds = vDBHelper.Execute();
                return ds;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public DataSet GetNonNetworkHospetalDetails(int ProviderID, int CityID, string ProviderName, string Location, int PINCode)
        {
            try
            {
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "USP_GetNonNetworkHospetalDetails");
                vDBHelper.mAddParameter("@ProviderID", SqlDbType.Int, ParameterDirection.Input, ProviderID);
                vDBHelper.mAddParameter("@CityID", SqlDbType.Int, ParameterDirection.Input, CityID);
                vDBHelper.mAddParameter("@ProviderName", SqlDbType.VarChar, ParameterDirection.Input, ProviderName);
                vDBHelper.mAddParameter("@Location", SqlDbType.VarChar, ParameterDirection.Input, Location);
                vDBHelper.mAddParameter("@PINCode", SqlDbType.Int, ParameterDirection.Input, PINCode);

                DataSet ds = vDBHelper.Execute();
                return ds;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public DataSet GetProviderRecentData()
        {
            try
            {
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "[USP_GetProviderRecentData]");
                DataSet ds = vDBHelper.Execute();
                return ds;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataSet GetNetWorkProviderVerificationData(int ProvReqID)
        {
            try
            {
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "USP_PROVREQ_RETRIEVE");
                vDBHelper.mAddParameter("@ProvReqID", SqlDbType.BigInt, ParameterDirection.Input, ProvReqID);
                DataSet ds = vDBHelper.Execute();

                DataTable Infrastructure = new DataTable();
                Infrastructure.Columns.Add("ID");
                Infrastructure.Columns.Add("LevelName");
                Infrastructure.Columns.Add("FacilityValue");
                Infrastructure.Columns.Add("DisplayName");
                Infrastructure.Columns.Add("DMSID");
                Infrastructure.Columns.Add("Type");
                Infrastructure.Columns.Add("ddValues");

                DataTable InnerTable = new DataTable();
                InnerTable.Columns.Add("ID");
                InnerTable.Columns.Add("Level3");
                InnerTable.Columns.Add("ProvFacilityID");

                DataTable table1 = ds.Tables[1];
                DataRow flagRow = table1.Rows[0];
                string flag = "", ddvalues = "";
                for (int i = 0; i < table1.Rows.Count - 1; i++)
                {
                    DataRow dr = table1.Rows[i];
                    DataRow nextRow = table1.Rows[i + 1];
                    if (dr["Level1"].ToString() == "Civil and Medical Infrastructure-General" && dr["Level2"].ToString() != "")
                    {
                        if (dr["Level2"].ToString() == nextRow["Level2"].ToString())
                        {
                            InnerTable.Rows.Add(dr["ID"], dr["Level3"], dr["ProvFacilityID"]);
                            flag = "true";
                        }
                        else if (flag == "true")
                        {
                            InnerTable.Rows.Add(dr["ID"], dr["Level3"], dr["ProvFacilityID"]);
                            flag = "false";
                            ddvalues = Newtonsoft.Json.JsonConvert.SerializeObject(InnerTable);
                            InnerTable.Clear();
                            Infrastructure.Rows.Add(dr["ParentID"], dr["Level2"], dr["FacilityValue"], dr["DisplayName"], dr["DMSID"], "DropDown", ddvalues);
                        }
                        else
                        {
                            InnerTable.Clear();
                            Infrastructure.Rows.Add(dr["ID"], dr["Level2"], dr["FacilityValue"], dr["DisplayName"], dr["DMSID"], "TextBox");
                        }
                    }
                }
                ds.Tables.Add(Infrastructure);

                return ds;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool CheckRohiniCode(string code, string PrcNo)
        {
            try
            {

                if (string.IsNullOrWhiteSpace(code))
                    return false;

                string query = @"SELECT TOP 1 1 
                         FROM Provider WITH (NOLOCK) 
                         WHERE RohiniCode = @code AND deleted=0
                         AND RohiniCode IS NOT NULL 
                         AND ISNULL(PRCNo, '0') <> ISNULL(@PrcNo, '0')";

                vDBHelper.mCreateCommand(CommandType.Text, query);
                vDBHelper.mAddParameter("@code", SqlDbType.VarChar, 50, ParameterDirection.Input, code);
                vDBHelper.mAddParameter("@PrcNo", SqlDbType.VarChar, 50, ParameterDirection.Input, PrcNo);


                var result = vDBHelper.ExecuteScalar();

                return result != null;
            }
            catch (Exception ex)
            {
                // 🔥 LOG THIS (VERY IMPORTANT)                
                throw;
            }
        }

        public DataSet SearchNetWorkProvider(int Type, int ReqID, string Name, string Location, int StateID, int CityID, string FromDate, string ToDate, string PRCNo, string RegLandlineNo, string RFromDate, string RToDate, string irda_num, string rohini_num, string pnr_num)
        {
            try
            {
                DataSet ds = new DataSet();
                string conditions = "";
                if (Name != "null")
                    conditions += " name like '%" + Name + "%'";


                if (Location != "null")
                {
                    if (conditions != "")
                        conditions += " and ";
                    conditions += " SearchLoaction like '%" + Location + "%'";
                }


                if (RegLandlineNo != "null")
                {
                    if (conditions != "")
                        conditions += " and ";
                    conditions += " RegLandlineNo like '%" + RegLandlineNo + "%'";
                }


                if (FromDate != "null" && ToDate != "null")
                {
                    if (conditions != "")
                        conditions += " and ";
                    conditions += " Createddatetime>=convert(datetime,'" + FromDate + "',101) and Createddatetime<=convert(datetime,'" + ToDate + "',101)";
                }


                if (CityID != 0)
                {
                    if (conditions != "")
                        conditions += " and ";
                    conditions += " CityID=" + CityID;
                }


                if (StateID != 0)
                {
                    if (conditions != "")
                        conditions += " and ";
                    conditions += " StateID=" + StateID;
                }


                if (PRCNo != "null")
                {
                    if (conditions != "")
                        conditions += " and ";
                    conditions += " PRCNo = '" + PRCNo + "'";
                }

                if (irda_num != "null")
                {
                    if (conditions != "")
                        conditions += " and ";
                    conditions += " IRDACode = '" + irda_num + "'";
                }
                if (rohini_num != "null")
                {
                    if (conditions != "")
                        conditions += " and ";
                    conditions += " RohiniCode = '" + rohini_num + "'";
                }
                if (pnr_num != "null")
                {
                    if (conditions != "")
                        conditions += " and ";
                    //conditions += " PRNNo = '" + pnr_num + "'";
                    conditions += " ProviderID in (select distinct Provider_Id from ProviderPRN where PRNnumber='" + pnr_num + "')";
                }

                if (Type == 1)
                {

                    if (RFromDate != "null" && RToDate != "null")
                    {
                        if (conditions != "")
                            conditions += " and ";
                        conditions += " RequestDate>=convert(datetime,'" + RFromDate + "',101) and RequestDate<=convert(datetime,'" + RToDate + "',101)";
                    }


                    if (ReqID != 0)
                    {
                        if (conditions != "")
                            conditions += " and ";
                        conditions += " RequestID=" + ReqID;
                    }

                    string Query = "SELECT top 800  * ,1 as Ismou FROM VW_GetAllProviderDetailsForSearch where  ProviderMouID!=" + -1 + " and Deleted=0  and convert(datetime,'" + System.DateTime.Now.ToString("MM/dd/yyyy") + "',101) between convert(datetime, StartDate,101) and convert(datetime, EndDate,101) " + " and " + conditions + " order by Createddatetime desc";
                    vDBHelper.mCreateCommand(CommandType.Text, Query);
                    ds = vDBHelper.Execute();
                }
                else if (Type == 2)
                {
                    if (ReqID != 0)
                    {
                        if (conditions != "")
                            conditions += " and ";
                        conditions += " RequestID=" + ReqID;
                    }
                    string Query = "SELECT top 800 *, case when EndDate is null then -1 else case when convert(datetime,'" + System.DateTime.Now.ToString("MM/dd/yyyy") + "',101) > convert(datetime, EndDate,101) then 0 else 1 end  end as Ismou  FROM VW_GetAllProviderDetailsForSearch where (convert(datetime,'" + System.DateTime.Now.ToString("MM/dd/yyyy") + "',101) < convert(datetime, StartDate,101) or (convert(datetime,'" + System.DateTime.Now.ToString("MM/dd/yyyy") + "',101) > convert(datetime, EndDate,101) or  EndDate is null)) and  " + conditions + " order by Createddatetime desc";
                    vDBHelper.mCreateCommand(CommandType.Text, Query);
                    ds = vDBHelper.Execute();
                }
                else if (Type == 3)
                {
                    if (RFromDate != "null" && RToDate != "null")
                    {
                        if (conditions != "")
                            conditions += " and ";
                        conditions += " RequestDate>=convert(datetime,'" + RFromDate + "',101) and RequestDate<=convert(datetime,'" + RToDate + "',101)";
                    }


                    if (ReqID != 0)
                    {
                        if (conditions != "")
                            conditions += " and ";
                        conditions += " RequestID=" + ReqID;
                    }
                    string Query = "SELECT  top 800 *, case when EndDate is null then -1 else case when convert(datetime,'" + System.DateTime.Now.ToString("MM/dd/yyyy") + "',101) > convert(datetime, EndDate,101) then 0 else 1 end end as Ismou FROM VW_GetAllProviderDetailsForSearch where  Deleted=0  and " + conditions + " order by Createddatetime desc";
                    vDBHelper.mCreateCommand(CommandType.Text, Query);
                    ds = vDBHelper.Execute();
                }
                return ds;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataSet SearchNetWorkProviderInsurerSpecific(int userID, int Type, int ReqID, string Name, string Location, int StateID, int CityID, string FromDate, string ToDate, string PRCNo, string RegLandlineNo, string RFromDate, string RToDate, string irda_num, string rohini_num, string pnr_num)
        {
            try
            {
                DataSet ds = new DataSet();
                string conditions = "";
                if (Name != "null")
                    conditions += " name like '%" + Name + "%'";


                if (Location != "null")
                {
                    if (conditions != "")
                        conditions += " and ";
                    conditions += " SearchLoaction like '%" + Location + "%'";
                }


                if (RegLandlineNo != "null")
                {
                    if (conditions != "")
                        conditions += " and ";
                    conditions += " RegLandlineNo like '%" + RegLandlineNo + "%'";
                }


                if (FromDate != "null" && ToDate != "null")
                {
                    if (conditions != "")
                        conditions += " and ";
                    conditions += " Createddatetime>=convert(datetime,'" + FromDate + "',101) and Createddatetime<=convert(datetime,'" + ToDate + "',101)";
                }


                if (CityID != 0)
                {
                    if (conditions != "")
                        conditions += " and ";
                    conditions += " CityID=" + CityID;
                }


                if (StateID != 0)
                {
                    if (conditions != "")
                        conditions += " and ";
                    conditions += " StateID=" + StateID;
                }


                if (PRCNo != "null")
                {
                    if (conditions != "")
                        conditions += " and ";
                    conditions += " PRCNo = '" + PRCNo + "'";
                }

                if (irda_num != "null")
                {
                    if (conditions != "")
                        conditions += " and ";
                    conditions += " IRDACode = '" + irda_num + "'";
                }
                if (rohini_num != "null")
                {
                    if (conditions != "")
                        conditions += " and ";
                    conditions += " RohiniCode = '" + rohini_num + "'";
                }
                if (pnr_num != "null")
                {
                    if (conditions != "")
                        conditions += " and ";
                    //conditions += " PRNNo = '" + pnr_num + "'";
                    conditions += " ProviderID in (select distinct Provider_Id from ProviderPRN where PRNnumber='" + pnr_num + "')";
                }

                if (userID != 0)
                {
                    if (conditions != "")
                        conditions += " and ";
                    conditions += "MouEntityID in (Select distinct IssueID from lnk_UserIssuingAuthority with (nolock) where Deleted = 0 and UserID = " + userID + ")";
                }
                if (Type == 1)
                {

                    if (RFromDate != "null" && RToDate != "null")
                    {
                        if (conditions != "")
                            conditions += " and ";
                        conditions += " RequestDate>=convert(datetime,'" + RFromDate + "',101) and RequestDate<=convert(datetime,'" + RToDate + "',101)";
                    }


                    if (ReqID != 0)
                    {
                        if (conditions != "")
                            conditions += " and ";
                        conditions += " RequestID=" + ReqID;
                    }


                    string Query = "SELECT top 800  * ,1 as Ismou FROM VW_GetAllProviderDetailsForSearch where  ProviderMouID!=" + -1 + " and Deleted=0  and convert(datetime,'" + System.DateTime.Now.ToString("MM/dd/yyyy") + "',101) between convert(datetime, StartDate,101) and convert(datetime, EndDate,101) " + " and " + conditions + " order by Createddatetime desc";
                    vDBHelper.mCreateCommand(CommandType.Text, Query);
                    ds = vDBHelper.Execute();
                }
                else if (Type == 2)
                {
                    if (ReqID != 0)
                    {
                        if (conditions != "")
                            conditions += " and ";
                        conditions += " RequestID=" + ReqID;
                    }
                    string Query = "SELECT top 800 *, case when EndDate is null then -1 else case when convert(datetime,'" + System.DateTime.Now.ToString("MM/dd/yyyy") + "',101) > convert(datetime, EndDate,101) then 0 else 1 end  end as Ismou  FROM VW_GetAllProviderDetailsForSearch with (nolock) where (convert(datetime,'" + System.DateTime.Now.ToString("MM/dd/yyyy") + "',101) < convert(datetime, StartDate,101) or (convert(datetime,'" + System.DateTime.Now.ToString("MM/dd/yyyy") + "',101) > convert(datetime, EndDate,101) or  EndDate is null)) and  " + conditions + " order by Createddatetime desc";
                    vDBHelper.mCreateCommand(CommandType.Text, Query);
                    ds = vDBHelper.Execute();
                }
                else if (Type == 3)
                {
                    if (RFromDate != "null" && RToDate != "null")
                    {
                        if (conditions != "")
                            conditions += " and ";
                        conditions += " RequestDate>=convert(datetime,'" + RFromDate + "',101) and RequestDate<=convert(datetime,'" + RToDate + "',101)";
                    }


                    if (ReqID != 0)
                    {
                        if (conditions != "")
                            conditions += " and ";
                        conditions += " RequestID=" + ReqID;
                    }
                    string Query = "SELECT  top 800 *, case when EndDate is null then -1 else case when convert(datetime,'" + System.DateTime.Now.ToString("MM/dd/yyyy") + "',101) > convert(datetime, EndDate,101) then 0 else 1 end end as Ismou FROM VW_GetAllProviderDetailsForSearch where  Deleted=0  and " + conditions + " order by Createddatetime desc";
                    vDBHelper.mCreateCommand(CommandType.Text, Query);
                    ds = vDBHelper.Execute();
                }
                return ds;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public DataSet GetProviderEntitypes()
        {
            try
            {
                DataSet ds;
                vDBHelper.mCreateCommand(CommandType.Text, "select ID,Name from MST_PROPERTYVALUES where PropertyID=44 and id in (166,167,169)");
                ds = vDBHelper.Execute();
                return ds;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataSet GetIssuingAuthorityDetails()
        {
            try
            {
                DataSet ds;
                vDBHelper.mCreateCommand(CommandType.Text, "select ID,Name from Mst_IssuingAuthority");
                ds = vDBHelper.Execute();
                return ds;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataSet GetPayerDetails()
        {
            try
            {
                DataSet ds;
                vDBHelper.mCreateCommand(CommandType.Text, "select ID,Name from Mst_Payer");
                ds = vDBHelper.Execute();
                return ds;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataSet GetCorporateDetails()
        {
            try
            {
                DataSet ds;
                vDBHelper.mCreateCommand(CommandType.Text, "select ID,Name from Mst_Corporate where Deleted=0");
                ds = vDBHelper.Execute();
                return ds;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataSet GetProviderMouSearchDetails(int ProviderID)
        {
            try
            {
                string Query = "  select * from ("
                                  + "select p.Name ,pm.ProviderID,pm.ID as ProviderMOUID,pm.MOUTypeID_P44,pm.Deleted,pm.MOUEntityID,pm.PRCNo,pm.MOUNo,pm.StartDate"
                                  + ",pm.EndDate,pm.ExpiryDate,pm.MOUDiscountPerc,pm.MOUDiscountAbs,pm.EffectiveDate,pm.DMSID,pm.TariffDMSID,pm.PackageDMSID,pm.isLatest,"
                                  + "ROW_NUMBER() OVER(PARTITION BY MOUEntityID,pm.ProviderID ORDER BY pm.EndDate DESC) AS Rowno,mt.Name as MOUType,"
                                  + "case when pm.MOUTypeID_P44=166 then 'TPA' when  pm.MOUTypeID_P44=167 then ms.Name when pm.MOUTypeID_P44=169 then mc.Name  end as EntityType"
                                  + " from Provider p"
                                  + " inner join ProviderMOU pm on pm.ProviderID=p.ID and pm.Deleted=0"
                                  + " left join MST_PROPERTYVALUES mt on mt.id=pm.MOUTypeID_P44"
                                  + " left join Mst_IssuingAuthority ms on ms.ID=pm.MOUEntityID"
                                  + " left join Mst_Corporate mc on mc.id=pm.MOUEntityID"
                                  + " where p.Deleted=0  and pm.ProviderID=" + ProviderID + ")as p where p.Rowno=1";
                vDBHelper.mCreateCommand(CommandType.Text, Query);
                DataSet ds = vDBHelper.Execute();
                return ds;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region Communication
        public DataSet GetEmailCommunicationDetails(string Entity_To, string Entity_CC, string Entity_BCC, long ProviderID, string SMS_To, int StageID)
        {
            DataSet ds = null;
            try
            {
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "Usp_GetEmailids_Provider");
                vDBHelper.mAddParameter("@Entity_To", SqlDbType.VarChar, 30, ParameterDirection.Input, Entity_To);
                vDBHelper.mAddParameter("@Entity_CC", SqlDbType.VarChar, 30, ParameterDirection.Input, Entity_CC);
                vDBHelper.mAddParameter("@Entity_BCC", SqlDbType.VarChar, 30, ParameterDirection.Input, Entity_BCC);
                vDBHelper.mAddParameter("@ProviderReqID", SqlDbType.BigInt, ParameterDirection.Input, ProviderID);
                vDBHelper.mAddParameter("@StageID", SqlDbType.Int, ParameterDirection.Input, StageID);
                vDBHelper.mAddParameter("@SMS_To", SqlDbType.VarChar, 30, ParameterDirection.Input, SMS_To);
                vDBHelper.mAddParameter("@Type", SqlDbType.TinyInt, ParameterDirection.Input, 0);

                ds = vDBHelper.Execute();
                return ds;
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }
        #endregion

        #region Provider FlagDeatils
        public string SaveProviderFlagDetails(long ProviderID, int MouTypeID, int MOUEntityID, int FlagID, string EffectiveFrom, string EffectiveTo, bool ClaimProcessing, bool InsuranceApprovalRequired, int ServiceType, string Remarks, int UserRegionID)
        {
            //DataSet ds = null;
            try
            {
                string Query = "insert into ProviderCategory (ProviderID,EntityTypeID_P6,RelevantID,providerStatusID,ProviderServiceID,StartDate,EndDate,ClaimProcessing,IsInsuranceApprovalRequired,Description,Deleted,Createddatetime,CreatedOperatorID)";
                Query += " values (@ProviderID,@EntityTypeID_P6,@RelevantID,@providerStatusID,@ProviderServiceID,@StartDate,case when @EndDate='' then null else @EndDate end  ,@ClaimProcessing,@IsInsuranceApprovalRequired,@Description,@Deleted,@Createddatetime,@CreatedOperatorID);" + "";
                vDBHelper.mCreateCommand(CommandType.Text, Query);
                vDBHelper.mAddParameter("@ProviderID", SqlDbType.BigInt, ParameterDirection.Input, ProviderID);
                vDBHelper.mAddParameter("@EntityTypeID_P6", SqlDbType.Int, ParameterDirection.Input, MouTypeID);
                vDBHelper.mAddParameter("@RelevantID", SqlDbType.BigInt, ParameterDirection.Input, MOUEntityID);
                vDBHelper.mAddParameter("@providerStatusID", SqlDbType.Int, ParameterDirection.Input, FlagID);
                vDBHelper.mAddParameter("@ProviderServiceID", SqlDbType.Int, ParameterDirection.Input, ServiceType);

                if (EffectiveFrom == "" || EffectiveFrom == null)
                    vDBHelper.mAddParameter("@StartDate", SqlDbType.DateTime, ParameterDirection.Input, DBNull.Value);
                else
                    vDBHelper.mAddParameter("@StartDate", SqlDbType.DateTime, ParameterDirection.Input, Convert.ToDateTime(EffectiveFrom));
                if (EffectiveTo == "" || EffectiveTo == null)
                    vDBHelper.mAddParameter("@EndDate", SqlDbType.DateTime, ParameterDirection.Input, DBNull.Value);
                else
                    vDBHelper.mAddParameter("@EndDate", SqlDbType.DateTime, ParameterDirection.Input, Convert.ToDateTime(EffectiveTo));
                //vDBHelper.mAddParameter("@ExpiryDate", SqlDbType.DateTime, 30, ParameterDirection.Input, EffectiveFrom);

                vDBHelper.mAddParameter("@ClaimProcessing", SqlDbType.Bit, ParameterDirection.Input, ClaimProcessing);
                vDBHelper.mAddParameter("@IsInsuranceApprovalRequired", SqlDbType.Bit, ParameterDirection.Input, InsuranceApprovalRequired);
                vDBHelper.mAddParameter("@Description", SqlDbType.VarChar, 150, ParameterDirection.Input, Remarks);
                vDBHelper.mAddParameter("@Deleted", SqlDbType.Int, ParameterDirection.Input, 0);
                vDBHelper.mAddParameter("@Createddatetime", SqlDbType.DateTime, ParameterDirection.Input, System.DateTime.Now);
                vDBHelper.mAddParameter("@CreatedOperatorID", SqlDbType.Int, ParameterDirection.Input, UserRegionID);
                // vDBHelper.mAddParameter("@Reason", SqlDbType.VarChar, 250, ParameterDirection.Input, Reason);
                vDBHelper.ExecuteNonQuery();
                return "data saved successfully";
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        public DataTable GetProviderFlagDetails(long ProviderID, int EntityTypeID_P6, long RelevantID, DateTime? CurrentDate)
        {
            DataSet ds = null;
            try
            {
                if (CurrentDate != null)
                    vDBHelper.mCreateCommand(CommandType.Text, "select * from ProviderCategory where ProviderID=@ProviderID and EntityTypeID_P6=@EntityTypeID_P6 and RelevantID=@RelevantID and @CurrentDate between StartDate and EndDate and Deleted=0  ");
                else
                    vDBHelper.mCreateCommand(CommandType.Text, "select * from ProviderCategory where ProviderID=@ProviderID and EntityTypeID_P6=@EntityTypeID_P6 and RelevantID=@RelevantID  and Deleted=0");
                vDBHelper.mAddParameter("@ProviderID", SqlDbType.BigInt, 30, ParameterDirection.Input, ProviderID);
                vDBHelper.mAddParameter("@EntityTypeID_P6", SqlDbType.Int, 30, ParameterDirection.Input, EntityTypeID_P6);
                vDBHelper.mAddParameter("@RelevantID", SqlDbType.Int, 30, ParameterDirection.Input, RelevantID);
                if (CurrentDate != null)
                    vDBHelper.mAddParameter("@CurrentDate", SqlDbType.DateTime, ParameterDirection.Input, CurrentDate);

                DataTable dt = vDBHelper.ExecuteDT();
                return dt;

            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        public string DeleteProviderFlagDetails(long ProviderFlagID, string Remarks, long UserRegionID)
        {
            DataSet ds = null;
            try
            {
                string Query = "update ProviderCategory set Deleted=1 ,Description=Description+';'+@Remarks,DeletedDatetime=@Createddatetime,DeletedOperatorID=@CreatedOperatorID  where ID=@ProviderFlagID ";
                vDBHelper.mCreateCommand(CommandType.Text, Query);
                vDBHelper.mAddParameter("@ProviderFlagID", SqlDbType.Int, 30, ParameterDirection.Input, ProviderFlagID);
                vDBHelper.mAddParameter("@Remarks", SqlDbType.VarChar, 1000, ParameterDirection.Input, Remarks);

                vDBHelper.mAddParameter("@Createddatetime", SqlDbType.DateTime, ParameterDirection.Input, System.DateTime.Now);
                vDBHelper.mAddParameter("@CreatedOperatorID", SqlDbType.Int, ParameterDirection.Input, UserRegionID);
                vDBHelper.ExecuteNonQuery();
                return "data saved successfully";

            }
            catch (Exception exception)
            {
                throw exception;
            }
        }
        #endregion


        public string SaveZones(string zonename)
        {
            try
            {
                string msg = string.Empty;
                string result = string.Empty;

                vDBHelper.mCreateCommand(CommandType.Text, "select * from Mst_Zones where Name=@zonename and Deleted=0");
                vDBHelper.mAddParameter("@zonename", SqlDbType.VarChar, ParameterDirection.Input, zonename);
                var chck = vDBHelper.Execute();

                if (chck != null && chck.Tables[0].Rows.Count != 0)
                {

                    msg = "Zones already Exists";
                }

                else
                {

                    vDBHelper.mCreateCommand(CommandType.Text, "insert into Mst_Zones(Name,Deleted)" + " values(@zonename,0)");
                    vDBHelper.mAddParameter("@zonename", SqlDbType.VarChar, ParameterDirection.Input, zonename);
                    vDBHelper.ExecuteNonQuery();
                    msg = "Zones Inserted Succesfully";

                }

                return msg;

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public DataSet GetInsurerZones(int InsurerId)
        {
            try
            {
                DataSet ds;
                vDBHelper.mCreateCommand(CommandType.Text, "Select *from  Lnk_InsurerZones where Issueid=@InsurerId and Deleted=0");
                vDBHelper.mAddParameter("@InsurerId", SqlDbType.Int, ParameterDirection.Input, InsurerId);
                ds = vDBHelper.Execute();
                return ds;



            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public string InsurerZoneWiseLinked(int InsurerId, string zones)
        {
            try
            {

                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "[Usp_Lnk_InsurerZones_Insert]");
                vDBHelper.mAddParameter("@IssuerId", SqlDbType.Int, ParameterDirection.Input, InsurerId);
                vDBHelper.mAddParameter("@List_Zones", SqlDbType.VarChar, ParameterDirection.Input, zones);
                vDBHelper.mAddParameter("@Msg", SqlDbType.VarChar, 1000, ParameterDirection.Output, "");
                string strReturn;
                vDBHelper.ExecuteNonQuery(out strReturn, "@Msg");
                return strReturn;


            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public DataTable PNPSearchNetWorkProvider(long? RequestID, string Name, string Location, int? StateID, int? CityID, string PRCNo, int? IssueID, int CskIsLinked)
        {

            DataTable dtInbox = null;
            try
            {
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "USP_PNPProvSearch");
                vDBHelper.mAddParameter("@Name", SqlDbType.VarChar, 100, ParameterDirection.Input, (Name == "") ? null : Name);
                vDBHelper.mAddParameter("@Location", SqlDbType.VarChar, 100, ParameterDirection.Input, Location == "" ? null : Location);
                vDBHelper.mAddParameter("@PRCNo", SqlDbType.VarChar, 10, ParameterDirection.Input, PRCNo == "" ? null : PRCNo);
                vDBHelper.mAddParameter("@StateID", SqlDbType.Int, ParameterDirection.Input, StateID);
                vDBHelper.mAddParameter("@CityID", SqlDbType.Int, ParameterDirection.Input, CityID);
                vDBHelper.mAddParameter("@ProviderID", SqlDbType.BigInt, ParameterDirection.Input, RequestID);
                vDBHelper.mAddParameter("@IssueID", SqlDbType.Int, ParameterDirection.Input, IssueID);
                vDBHelper.mAddParameter("@CskIsLinked", SqlDbType.Int, ParameterDirection.Input, CskIsLinked);
                dtInbox = vDBHelper.ExecuteDT();

                return dtInbox;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string PPNInsert(int IssueID, string List_Provider, int CreatedUserRegionID, int CskIsLinked)
        {
            try
            {
                string message = "";
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "Usp_Lnk_InsurerProvider_Insert");
                vDBHelper.mAddParameter("@IssueID", SqlDbType.Int, ParameterDirection.Input, IssueID);
                vDBHelper.mAddParameter("@List_Provider", SqlDbType.VarChar, ParameterDirection.Input, List_Provider == "" ? null : List_Provider);
                vDBHelper.mAddParameter("@CreatedUserRegionID", SqlDbType.Int, ParameterDirection.Input, CreatedUserRegionID);
                vDBHelper.mAddParameter("@CskIsLinked", SqlDbType.Int, ParameterDirection.Input, CskIsLinked);
                vDBHelper.mAddParameter("@Msg", SqlDbType.VarChar, 150, ParameterDirection.Output, "");
                vDBHelper.ExecuteNonQuery(out message, "@Msg");
                return message;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable GetProviderProcedureWisePackage(Int64 ProviderID, Int64 MOUID)
        {
            try
            {
                DataTable dt = new DataTable();
                vDBHelper.mCreateCommand(CommandType.Text, "select PP.TPAProcID AS Procedure_Id ,TPA.Level3 ProcedureName ,TPA.Code,TPA.Level1  SPECIALITY ,PP.Discount,mc.Level2,PP.Amount  from ProviderPackage PP inner join TPAProcedures TPA ON PP.TPAProcID=TPA.ID and PP.Deleted=0  inner join Mst_Facility mc on mc.id=pp.FacilityID where PP.Discount>0 and ProviderID=@ProviderID AND MOUID=@MOUID and PP.Discount <> isnull((select PackagePercentage from ProviderMOU where ProviderID=@ProviderID and ID=@MOUID),0)");
                vDBHelper.mAddParameter("@ProviderID", SqlDbType.BigInt, ParameterDirection.Input, ProviderID);
                vDBHelper.mAddParameter("@MOUID", SqlDbType.BigInt, ParameterDirection.Input, MOUID);
                dt = vDBHelper.ExecuteDT();
                return dt;
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
        }

        public DataTable CheckPackageDiscount(Int64 ProviderID, string MOUID)
        {
            try
            {
                DataTable dt = new DataTable();
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "Usp_GetPackageDiscount");
                vDBHelper.mAddParameter("@ProviderID", SqlDbType.BigInt, ParameterDirection.Input, ProviderID);
                vDBHelper.mAddParameter("@MOUID", SqlDbType.VarChar, ParameterDirection.Input, MOUID);
                dt = vDBHelper.ExecuteDT();
                return dt;
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
        }

        public DataTable CheckServiceDiscount(Int64 ProviderID, string MOUID)
        {
            try
            {
                DataTable dt = new DataTable();
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "Usp_GetServiceDiscount");
                vDBHelper.mAddParameter("@ProviderID", SqlDbType.BigInt, ParameterDirection.Input, ProviderID);
                vDBHelper.mAddParameter("@MOUID", SqlDbType.VarChar, ParameterDirection.Input, MOUID);
                dt = vDBHelper.ExecuteDT();
                return dt;
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
        }

        public DataSet GetSpecialityDetails()
        {
            try
            {

                string Query = "SELECT ID,Level1 As Name FROM TPAProcedures WHERE ParentID=0 and Deleted=0";
                vDBHelper.mCreateCommand(CommandType.Text, Query);
                DataSet ds = vDBHelper.Execute();
                return ds;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataSet GetFacilitiesDetails()
        {
            try
            {

                string Query = "select ID,Level2 As Name from Mst_Facility where ParentID=1 and Deleted=0 order by Level2";
                vDBHelper.mCreateCommand(CommandType.Text, Query);
                DataSet ds = vDBHelper.Execute();
                return ds;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataSet GetHosptialTypes()
        {
            try
            {

                string Query = "select ID,Name from Mst_PropertyValues where PropertyID=86 and Deleted=0 order by ID";
                vDBHelper.mCreateCommand(CommandType.Text, Query);
                DataSet ds = vDBHelper.Execute();
                return ds;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataSet GetNABHAccreditionTypes()
        {
            try
            {

                string Query = "select ID, Name from Mst_PropertyValues where PropertyID=85 and Deleted=0";
                vDBHelper.mCreateCommand(CommandType.Text, Query);
                DataSet ds = vDBHelper.Execute();
                return ds;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataSet GetProcedureBasedSumoSpeciality(string oProcedureId)
        {
            try
            {

                //string Query = "select ID,Level3 Name from TPAProcedures where deleted = 0 and code != '''' and level3 is not null and level3<> '''' and ParentId in( select stringvalue from fn_split(@oProcedureId,',') ) order by ID";
                string Query = "select ID,Level3 Name,ParentID from TPAProcedures where ParentID!=0 and ParentID in (select ID from TPAProcedures where ParentID in( select stringvalue from fn_split(@oProcedureId,',') )) and  Deleted=0 and code>0  and level3 is not null and level3<>'' Order by Level3 ";
                //vDBHelper.mCreateCommand(CommandType.StoredProcedure, "Usp_GetProcedureList");

                vDBHelper.mCreateCommand(CommandType.Text, Query);
                vDBHelper.mAddParameter("@oProcedureId", SqlDbType.VarChar, ParameterDirection.Input, oProcedureId);

                DataSet ds = vDBHelper.Execute();
                return ds;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string UpdateProviderMOUAllProcedureDiscounts(string _mouId, decimal _allDiscounts, int _updateDiscountType, long _providerIdValue, int UserRegionID, string remarks)
        {
            string msg = string.Empty;
            try
            {
                //DataTable dt = new DataTable();
                //vDBHelper.mCreateCommand(CommandType.Text, "update ProviderMOU set PackagePercentage=@AllDiscounts,DiscountUpdateType=@UpdateDiscountType  where ID=@MOUID");
                //Usp_ProviderPackageDiscount_Update
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "Usp_ProviderPackageDiscount_Update");
                vDBHelper.mAddParameter("@MOUID", SqlDbType.VarChar, ParameterDirection.Input, _mouId);
                vDBHelper.mAddParameter("@Discount", SqlDbType.Decimal, ParameterDirection.Input, _allDiscounts);
                vDBHelper.mAddParameter("@DiscountType", SqlDbType.Int, ParameterDirection.Input, _updateDiscountType);
                vDBHelper.mAddParameter("@ProviderID", SqlDbType.BigInt, ParameterDirection.Input, _providerIdValue);
                vDBHelper.mAddParameter("@PackageDiscountRemarks", SqlDbType.VarChar, 500, ParameterDirection.Input, remarks);
                vDBHelper.mAddParameter("@ModifiedUserRegionID", SqlDbType.Int, ParameterDirection.Input, UserRegionID);
                vDBHelper.mAddParameter("@msg", SqlDbType.VarChar, 500, ParameterDirection.Output, DBNull.Value);

                //dt = vDBHelper.ExecuteDT();
                //DataSet ds = vDBHelper.Execute();
                //DataTable dt2 = GetUpdatedProviderMOUAllProcedureDiscounts(_mouId);
                //return ds;

                vDBHelper.ExecuteNonQuery(out msg, "@msg");
                return msg;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //public DataTable GetUpdatedProviderMOUAllProcedureDiscounts(int _mouId)
        //{
        //    try
        //    {
        //        DataTable dt = new DataTable();
        //        string sqlQuery = "SELECT ID,ProviderID,MOUTypeID_P44,MOUEntityID,PRCNo,MOUNo,StartDate,EndDate,ExpiryDate,MOUDiscountPerc,MOUDiscountAbs,MOUReceivedDate,EffectiveDate,DMSID,TariffDMSID,PackageDMSID,isLatest,Deleted,Createddatetime,"
        //            + " CreatedUserRegionID,DeletedUserRegionID,Deleteddatetime,IPPercentage,OPPercentage,isnull(PackagePercentage,0)as PackagePercentage,AgreementType,AgreementID,ModifiedUserRegionID,Modifieddatetime,DiscountUpdateType"
        //            + "  FROM dbo.ProviderMOU where  ID=@MOUID"; 
        //        vDBHelper.mCreateCommand(CommandType.Text, sqlQuery);
        //        vDBHelper.mAddParameter("@MOUID", SqlDbType.Int, ParameterDirection.Input, _mouId);
        //        dt = vDBHelper.ExecuteDT();
        //        return dt;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        public DataTable CheckFlag(Int64 ProviderID)
        {
            try
            {
                vDBHelper.mCreateCommand(CommandType.Text, "select Count(case when pc.providerStatusID=2 then pc.ID end) as Excluded,Count(case when pc.providerStatusID=1 then pc.ID end) as Cautious from  ProviderCategory pc where  (datediff(day,convert(date,getdate()),convert(date,pc.EndDate))>=0 or pc.EndDate is null) and  pc.ProviderID=@ProviderID and Deleted=0");
                vDBHelper.mAddParameter("@ProviderID", SqlDbType.BigInt, ParameterDirection.Input, ProviderID);
                DataTable dt = vDBHelper.ExecuteDT();
                return dt;
            }
            catch (Exception Ex)
            {
                throw;
            }

        }

        public DataTable GetPackageConsolidatedDiscount(Int64 ProviderID, Int64 MOUID)
        {

            try
            {
                DataTable dt = new DataTable();
                string StrQry = "select id TPAProcID, Code as Category, Level3 Name, Level1 as SPECIALITY, isnull((select PackagePercentage from ProviderMOU where ProviderID = @ProviderID and Id = @MOUID), 0.00) as Discount from TPAProcedures"
                     + " where deleted = 0 and code> 0 and level3 is not null and level3<> '' and id not in (select TPAProcID from ProviderPackage where ProviderID = @ProviderID and MOUID = @MOUID) union  select Tpa.id TPAProcID, Tpa.Code"
                     + " as Category,Tpa.Level3 Name, Tpa.Level1 as SPECIALITY,case when Pp.Discount=0  then isnull((select PackagePercentage from ProviderMOU where ProviderID=@ProviderID and ID=@MOUID),0.00) else Pp.Discount end  as Discount "
                     + " from ProviderPackage Pp inner join TPAProcedures Tpa on Pp.TPAProcID = Tpa.ID and Pp.id in (select  ID from (select TPAProcID,max(Id)as ID from ProviderPackage where ProviderID = @ProviderID  and MOUID = @MOUID  group by TPAProcID)as ProviderPackage) ";

                vDBHelper.mCreateCommand(CommandType.Text, StrQry);
                vDBHelper.mAddParameter("@ProviderID", SqlDbType.BigInt, ParameterDirection.Input, ProviderID);
                vDBHelper.mAddParameter("@MOUID", SqlDbType.BigInt, ParameterDirection.Input, MOUID);

                dt = vDBHelper.ExecuteDT();


                return dt;
            }
            catch (Exception Ex)
            {
                throw Ex;
            }

        }

        public string UpdateProviderMOUDiscount(decimal IPDisscount, decimal OPDisscount, string MOUID, Int64 _ProviderId, int UserRegionID, DateTime? EffectiveDate, string remarks, string categoryDiscountsJson)
        {
            string msg = string.Empty;
            try
            {
                //vDBHelper.mCreateCommand(CommandType.Text, "update ProviderMOU set IPPercentage=@IPDisscount,OPPercentage=@OPDisscount where  deleted=0 and ID=@MOUID");
                // vDBHelper.mCreateCommand(CommandType.StoredProcedure, "Usp_ProviderMOUDiscount_Update");
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "Usp_ProviderMOUDiscount_Update_Categorieswise");
                vDBHelper.mAddParameter("@ProviderID", SqlDbType.BigInt, ParameterDirection.Input, _ProviderId);
                vDBHelper.mAddParameter("@MOUID", SqlDbType.VarChar, ParameterDirection.Input, MOUID);
                vDBHelper.mAddParameter("@IPDiscount", SqlDbType.Decimal, ParameterDirection.Input, IPDisscount);
                vDBHelper.mAddParameter("@OPDiscount", SqlDbType.Decimal, ParameterDirection.Input, OPDisscount);
                vDBHelper.mAddParameter("@ServiceDiscountRemarks", SqlDbType.VarChar, 500, ParameterDirection.Input, remarks);
                vDBHelper.mAddParameter("@ModifiedUserRegionID", SqlDbType.Int, ParameterDirection.Input, UserRegionID);
                vDBHelper.mAddParameter("@EffectiveDate", SqlDbType.DateTime, ParameterDirection.Input, (EffectiveDate == null ? DateTime.Now.Date : EffectiveDate));
                vDBHelper.mAddParameter("@CategoryDiscounts", SqlDbType.NVarChar, ParameterDirection.Input, categoryDiscountsJson); // NVARCHAR(MAX)

                vDBHelper.mAddParameter("@msg", SqlDbType.VarChar, 1000, ParameterDirection.Output, DBNull.Value);

                //DataSet ds = vDBHelper.ExecutewithSingleparam(out strReturn, "@Msg");
                //DataSet ds = vDBHelper.Execute();

                vDBHelper.ExecuteNonQuery(out msg, "@msg");
                return msg;
            }
            catch (Exception Ex)
            {
                throw;
            }
        }
        public DataSet InsertProviderFileRecordRemarks(string file, long EntityID, int EntityTypeID, ref string SystemFileName, ref string FullPath, int ReceivedModeID = 86, string Remarks = null, string filetype = null, long ProviderID = 0, int DMFiD = 0, int UserRegionID = 0, int mouid = 0, int filesize = 0, string fileExtention = null)
        {
            try
            {
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "Usp_ProviderFileRecordRemarks_Insert");

                vDBHelper.mAddParameter("@File", SqlDbType.VarChar, ParameterDirection.Input, file);
                vDBHelper.mAddParameter("@EntityID", SqlDbType.Int, ParameterDirection.Input, EntityID);
                vDBHelper.mAddParameter("@EntityTypeID", SqlDbType.Int, ParameterDirection.Input, EntityTypeID);
                vDBHelper.mAddParameter("@SystemFileName", SqlDbType.VarChar, ParameterDirection.Input, SystemFileName);
                vDBHelper.mAddParameter("@DNSName", SqlDbType.VarChar, ParameterDirection.Input, "200.200.201.85");
                vDBHelper.mAddParameter("@FullPath", SqlDbType.VarChar, ParameterDirection.Input, FullPath);
                vDBHelper.mAddParameter("@ReceivedModeID", SqlDbType.Int, ParameterDirection.Input, ReceivedModeID);
                vDBHelper.mAddParameter("@FilseSizeinKB", SqlDbType.Int, ParameterDirection.Input, filesize);
                vDBHelper.mAddParameter("@FilseExtention", SqlDbType.VarChar, ParameterDirection.Input, fileExtention);

                vDBHelper.mAddParameter("@ProviderID", SqlDbType.Int, ParameterDirection.Input, ProviderID);
                vDBHelper.mAddParameter("@FileTypeID", SqlDbType.VarChar, ParameterDirection.Input, filetype);
                vDBHelper.mAddParameter("@MOUID", SqlDbType.Int, ParameterDirection.Input, mouid);
                vDBHelper.mAddParameter("@Remarks", SqlDbType.VarChar, ParameterDirection.Input, Remarks);
                vDBHelper.mAddParameter("@DMFileId", SqlDbType.Int, ParameterDirection.Input, DMFiD);
                vDBHelper.mAddParameter("@CreatedUserRegionID", SqlDbType.Int, ParameterDirection.Input, UserRegionID);

                vDBHelper.mAddParameter("@Msg", SqlDbType.VarChar, 1000, ParameterDirection.Output, "");

                DataSet ds = vDBHelper.Execute();
                return ds;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public DataSet GetLoadProviderFileInfoByID(string providerId, string ID)
        {
            try
            {
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "Usp_GetProviderFileInfo");
                vDBHelper.mAddParameter("@ID", SqlDbType.BigInt, ParameterDirection.Input, Convert.ToInt64(ID));
                vDBHelper.mAddParameter("@ProviderID", SqlDbType.BigInt, ParameterDirection.Input, providerId);

                vDBHelper.mAddParameter("@Msg", SqlDbType.VarChar, 1000, ParameterDirection.Output, "");

                DataSet ds = vDBHelper.Execute();
                return ds;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataSet UpdateProviderFileInfo(Int64? ID, long providerId, string Remarks, int UserRegionID)
        {
            try
            {
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "Usp_ProviderFileInfo_Update");
                vDBHelper.mAddParameter("@ID", SqlDbType.BigInt, ParameterDirection.Input, Convert.ToInt64(ID));
                vDBHelper.mAddParameter("@ProviderID", SqlDbType.BigInt, ParameterDirection.Input, providerId);
                vDBHelper.mAddParameter("@Remarks", SqlDbType.VarChar, ParameterDirection.Input, Remarks);
                vDBHelper.mAddParameter("@ModifiedUserRegionID", SqlDbType.Int, ParameterDirection.Input, UserRegionID);

                vDBHelper.mAddParameter("@Msg", SqlDbType.VarChar, 1000, ParameterDirection.Output, "");

                DataSet ds = vDBHelper.Execute();
                return ds;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataSet InsertandupdateProcedureWiseDiscount(DataTable oPackageDiscountDetails, string _LocaMouID, Int64 _providerID, String _Remarks, int CreatedUserRegionID)
        {
            try
            {
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "Usp_SpecialitySpecific_ProviderPackageDiscount_Update");
                vDBHelper.mAddParameter("@SpecialitySpecificPackage", SqlDbType.Structured, ParameterDirection.Input, oPackageDiscountDetails);
                vDBHelper.mAddParameter("@MOUID", SqlDbType.VarChar, ParameterDirection.Input, _LocaMouID);
                vDBHelper.mAddParameter("@ProviderID", SqlDbType.BigInt, ParameterDirection.Input, _providerID);
                vDBHelper.mAddParameter("@Remarks", SqlDbType.VarChar, int.MaxValue, ParameterDirection.Input, _Remarks);
                vDBHelper.mAddParameter("@CreatedUserRegionID", SqlDbType.BigInt, ParameterDirection.Input, CreatedUserRegionID);
                vDBHelper.mAddParameter("@Msg", SqlDbType.VarChar, 1000, ParameterDirection.Output, DBNull.Value);
                //string strReturn;
                //vDBHelper.ExecuteNonQuery(out strReturn, "@Msg");
                //return strReturn;
                DataSet ds = vDBHelper.Execute();
                return ds;
            }
            catch (Exception Ex)
            {
                //return "Failed" + Ex.Message;
                throw;
            }
        }

        public DataSet InsertandupdateServicewiseTariff(DataTable oPackageDiscountDetails, string _LocaMouID, Int64 _providerID, String _Remarks, int CreatedUserRegionID)
        {
            try
            {
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "Usp_ServiceSpecific_ProviderTariff_Update");
                vDBHelper.mAddParameter("@ServiceSpecificMouTariff", SqlDbType.Structured, ParameterDirection.Input, oPackageDiscountDetails);
                vDBHelper.mAddParameter("@MOUID", SqlDbType.VarChar, ParameterDirection.Input, _LocaMouID);
                vDBHelper.mAddParameter("@ProviderID", SqlDbType.BigInt, ParameterDirection.Input, _providerID);
                vDBHelper.mAddParameter("@Remarks", SqlDbType.VarChar, int.MaxValue, ParameterDirection.Input, _Remarks);
                vDBHelper.mAddParameter("@CreatedUserRegionID", SqlDbType.BigInt, ParameterDirection.Input, CreatedUserRegionID);
                vDBHelper.mAddParameter("@Msg", SqlDbType.VarChar, 1000, ParameterDirection.Output, DBNull.Value);
                DataSet ds = vDBHelper.Execute();
                return ds;
            }
            catch (Exception Ex)
            {
                //return "Failed" + Ex.Message;
                throw;
            }
        }

        public DataSet GetLoadProviderFileInfo(string ProviderID)
        {
            try
            {
                string Query = "select PF.ID,pF.FileTypeID, PF.Remarks,CASE WHEN PF.ModifiedDatetime IS NULL THEN PF.CreatedDatetime ELSE PF.ModifiedDatetime END AS UpdateDate,DMSP.ID as DMSPID, DMSP.Name as FileNames from DeclarationFileInfo PF inner join DMSFileinfo_Provider DMSP on DMSP.ID=PF.DMFileId where PF.ProviderID=" + ProviderID + "  order by ID DESC";
                vDBHelper.mCreateCommand(CommandType.Text, Query);
                DataSet ds = vDBHelper.Execute();
                return ds;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string ProviderPNPInsert(int ProviderID, string List_Insurer, string List_UnchekInsurers, int CreatedUserRegionID)
        {
            try
            {
                string message = "";
                try
                {
                    vDBHelper.mCreateCommand(CommandType.StoredProcedure, "Usp_Lnk_ProviderInsurer_Insert");
                    vDBHelper.mAddParameter("@ProviderID", SqlDbType.Int, ParameterDirection.Input, ProviderID);
                    vDBHelper.mAddParameter("@List_Insurer", SqlDbType.VarChar, ParameterDirection.Input, List_Insurer == "" ? null : List_Insurer);
                    vDBHelper.mAddParameter("@List_UnchekInsurers", SqlDbType.VarChar, ParameterDirection.Input, List_UnchekInsurers == "" ? null : List_UnchekInsurers);
                    vDBHelper.mAddParameter("@CreatedUserRegionID", SqlDbType.Int, ParameterDirection.Input, CreatedUserRegionID);

                    vDBHelper.mAddParameter("@Msg", SqlDbType.VarChar, 150, ParameterDirection.Output, "");
                    vDBHelper.ExecuteNonQuery(out message, "@Msg");
                    return message;
                }
                catch (Exception exception)
                {
                    return "ErrorCode#1";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataSet ProviderLogin_User(string UserName, string Password, int type)
        {
            try
            {
                DataSet dt;
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "Usp_Provider_Login");
                vDBHelper.mAddParameter("@UserName", SqlDbType.VarChar, ParameterDirection.Input, UserName);
                vDBHelper.mAddParameter("@Password", SqlDbType.VarChar, ParameterDirection.Input, Password);
                vDBHelper.mAddParameter("@Type", SqlDbType.Int, ParameterDirection.Input, type);
                vDBHelper.mAddParameter("@Msg", SqlDbType.VarChar, ParameterDirection.Output, "");
                dt = vDBHelper.Execute();
                return dt;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        #region Early Payment Discount on Provider and Display On Claims Screen with Functionality(SP-1307)

        #region SaveEPDDetails
        public DataTable SaveEPDDetails(int IssueId, decimal EPD, string EPDRemarks, DateTime? EffectiveFrom, DateTime? EffectiveTo, long ProviderId, int UserRegionId, byte crudId, long EPDId, out string Message)
        {
            DataTable dtEPD = new DataTable();
            try
            {
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "usp_SaveEPDDetails");
                vDBHelper.mAddParameter("@IssueID ", SqlDbType.Int, ParameterDirection.Input, IssueId);
                vDBHelper.mAddParameter("@EPD", SqlDbType.Decimal, ParameterDirection.Input, EPD);
                vDBHelper.mAddParameter("@EPDRemarks", SqlDbType.VarChar, 250, ParameterDirection.Input, ((string.IsNullOrEmpty(EPDRemarks) || EPDRemarks == "null") ? null : EPDRemarks));
                vDBHelper.mAddParameter("@EffectiveFrom", SqlDbType.DateTime, ParameterDirection.Input, (EffectiveFrom == null ? DateTime.Now.Date : EffectiveFrom));
                vDBHelper.mAddParameter("@EffectiveTo", SqlDbType.DateTime, ParameterDirection.Input, (EffectiveTo == null ? null : EffectiveTo));
                vDBHelper.mAddParameter("@ProviderId", SqlDbType.BigInt, ParameterDirection.Input, ProviderId);
                vDBHelper.mAddParameter("@CreatedUserRegionID", SqlDbType.Int, ParameterDirection.Input, UserRegionId);
                vDBHelper.mAddParameter("@CRUDId", SqlDbType.TinyInt, ParameterDirection.Input, crudId);
                vDBHelper.mAddParameter("@EPDId", SqlDbType.BigInt, ParameterDirection.Input, EPDId);
                vDBHelper.mAddParameter("@Message", SqlDbType.VarChar, 8000, ParameterDirection.Output, DBNull.Value);
                dtEPD = vDBHelper.ExecuteDT(out Message, "@Message");
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return dtEPD;
        }
        #endregion

        #region GetEPDDetails
        public DataTable GetEPDDetails(long ProviderId)
        {
            DataTable dtEPD = new DataTable();
            try
            {
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "usp_GetEPDDetails");
                vDBHelper.mAddParameter("@ProviderId", SqlDbType.BigInt, ParameterDirection.Input, ProviderId);
                dtEPD = vDBHelper.ExecuteDT();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return dtEPD;
        }
        #endregion

        #endregion

        #region UploadPackage_OtherFiles
        public bool UploadPackage_OtherFiles(string FileURL, string FileName, long ProviderReqID, int fileStage, int FileFormat, int CreatedUserRegionID, int IsProvider)
        {
            try
            {
                string packageFileQuery = "UPDATE ProviderReqPackageNegationFileDetails SET IsLatest = 0, IsFinal = 0 WHERE ProviderReqID = @ProviderReqID AND Deleted = 0;" +
                                          " INSERT INTO ProviderReqPackageNegationFileDetails(FileURL, ProviderReqID, CreatedUserRegionID, CreatedDateTime, FileFormat, [FileName], IsLatest, IsFinal, Deleted, UploadedBy, FileStage)" +
                                          " VALUES(@FileURL, @ProviderReqID, @CreatedUserRegionID, GETDATE(), @FileFormat, @FileName, 1, 0, 0, @UploadedBy, @fileStage); ";
                vDBHelper.mCreateCommand(CommandType.Text, packageFileQuery);
                vDBHelper.mAddParameter("@ProviderReqID", SqlDbType.BigInt, ParameterDirection.Input, ProviderReqID);
                vDBHelper.mAddParameter("@fileStage", SqlDbType.Int, ParameterDirection.Input, fileStage);
                vDBHelper.mAddParameter("@UploadedBy", SqlDbType.Int, ParameterDirection.Input, IsProvider);
                vDBHelper.mAddParameter("@CreatedUserRegionID", SqlDbType.Int, ParameterDirection.Input, CreatedUserRegionID);
                vDBHelper.mAddParameter("@FileURL", SqlDbType.VarChar, ParameterDirection.Input, FileURL);
                vDBHelper.mAddParameter("@FileName", SqlDbType.VarChar, ParameterDirection.Input, FileName);
                vDBHelper.mAddParameter("@FileFormat", SqlDbType.VarChar, ParameterDirection.Input, FileFormat);
                vDBHelper.ExecuteNonQuery();
                return true;
            }
            catch (Exception Ex)
            {
                string error = Ex.Message;
                return false;
            }
        }
        #endregion

        #region Audit details of MOU,Hospitaldetails and Hospital Flag #SP3V-2407

        /// <summary>
        /// GetHosptalAuditDetails- Getting audit details of Hospital Details based on Provider ##SP3V-2407 ##Date:11-07-2023.
        /// </summary>
        /// <param name="ProviderID"></param>
        /// <returns>Hospital Flag Details as DataTable</returns>
        public DataTable GetHosptalAuditDetails(int ProviderID)
        {
            try
            {
                DataTable dt = new DataTable();

                try
                {
                    vDBHelper.mCreateCommand(CommandType.StoredProcedure, "USP_Get_AuditLog_HospitalDetails");
                    vDBHelper.mAddParameter("@ProviderId", SqlDbType.Int, ParameterDirection.Input, ProviderID);
                    dt = vDBHelper.ExecuteDT();
                    return dt;


                }
                catch (Exception exception)
                {
                    string errorMsg = exception.Message;
                    return dt;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// GetMOUAuditDetails- Getting audit details of MOU Details based on Provider ##SP3V-2407 ##Date:11-07-2023.
        /// </summary>
        /// <param name="ProviderID"></param>
        /// <returns>MOU Details as DataTable</returns>
        public DataTable GetMOUAuditDetails(int ProviderID)
        {

            DataTable dt = null;
            try
            {
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "USP_Get_AuditLog_MOUDetails");
                vDBHelper.mAddParameter("@providerId", SqlDbType.Int, ParameterDirection.Input, ProviderID);
                dt = vDBHelper.ExecuteDT();
                return dt;
            }
            catch (Exception Ex)
            {
                string errorMsg = Ex.Message;
                return dt;
            }

        }

        #endregion
        //SP3V-2243 Leena-----------------------------------
        public DataTable GetCAProviderConfigProc(Int64 ProviderId, Int16 FlgActivationStatus = 2)
        {
            try
            {
                DataTable dt = new DataTable();

                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "USP_CLA_ProviderConfig_FillProcedure");
                vDBHelper.mAddParameter("@ProviderId", SqlDbType.BigInt, ParameterDirection.Input, ProviderId);
                vDBHelper.mAddParameter("@FlgActivationStatus", SqlDbType.BigInt, ParameterDirection.Input, FlgActivationStatus);
                dt = vDBHelper.ExecuteDT();
                return dt;
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
        }
        public DataTable SaveProviderCAProc(Int64 ProviderId, DataTable dtProviderProc, int UserRegionId)
        {
            try
            {
                System.IO.StringWriter sw = new System.IO.StringWriter();
                string strprocedureid;

                dtProviderProc.TableName = "ProviderProc";
                dtProviderProc.WriteXml(sw);
                strprocedureid = sw.ToString();

                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "USP_CLA_ProviderConfig_SaveProcedure");
                vDBHelper.mAddParameter("@ProviderId", SqlDbType.BigInt, ParameterDirection.Input, ProviderId);
                vDBHelper.mAddParameter("@XmlProviderProc", SqlDbType.Xml, ParameterDirection.Input, strprocedureid);
                vDBHelper.mAddParameter("@CreatedBy", SqlDbType.Int, ParameterDirection.Input, UserRegionId);

                DataTable dt = vDBHelper.ExecuteDT();
                return dt;
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
        }
        //END SP3V-2243-------------------------------------------------


        //Tariff Upload - Leena SP3V-2777 -----------------------------------------------
        public DataSet GetTariffUploadProviderDetails(string PRCNO, long FormatType, long ProviderId, long MouId)
        {
            try
            {
                DataSet ds = new DataSet();
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "Usp_TariffUpload_FillDetails");
                vDBHelper.mAddParameter("@PRCNo", SqlDbType.VarChar, ParameterDirection.Input, PRCNO);
                vDBHelper.mAddParameter("@FormatType", SqlDbType.BigInt, ParameterDirection.Input, FormatType);

                vDBHelper.mAddParameter("@MouId", SqlDbType.BigInt, ParameterDirection.Input, MouId);
                vDBHelper.mAddParameter("@ProviderId", SqlDbType.BigInt, ParameterDirection.Input, ProviderId);

                ds = vDBHelper.Execute();
                return ds;
            }
            catch (Exception Ex)
            {
                throw;
            }

        }
        public DataSet GetTariffTemplate(string PRCNO, long ProviderId, long MouId, Int16 FormatType, Int16 TariffType)
        {
            try
            {
                DataSet ds = new DataSet();
                if (FormatType != 1)
                {
                    FormatType = 2;
                }
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "Usp_TariffUpload_DownloadTemplate");
                vDBHelper.mAddParameter("@PRCNo", SqlDbType.VarChar, ParameterDirection.Input, PRCNO);
                vDBHelper.mAddParameter("@MouId", SqlDbType.BigInt, ParameterDirection.Input, MouId);
                vDBHelper.mAddParameter("@ProviderId", SqlDbType.BigInt, ParameterDirection.Input, ProviderId);
                vDBHelper.mAddParameter("@FormatType", SqlDbType.BigInt, ParameterDirection.Input, FormatType);
                vDBHelper.mAddParameter("@TariffType", SqlDbType.BigInt, ParameterDirection.Input, TariffType);

                ds = vDBHelper.Execute();
                return ds;
            }
            catch (Exception Ex)
            {
                throw;
            }

        }
        public DataSet DownloadGIPSAMasterFormat(string PRCNO, long ProviderId, long MouId, Int16 FormatType, Int16 TariffType)
        {
            try
            {
                DataSet ds = new DataSet();
                if (FormatType != 1)
                {
                    FormatType = 2;
                }
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "USP_GetGipsaMasterData");
                vDBHelper.mAddParameter("@FormatType", SqlDbType.BigInt, ParameterDirection.Input, FormatType);
                ds = vDBHelper.Execute();
                return ds;
            }
            catch (Exception Ex)
            {
                throw;
            }

        }
        public string UploadRoomTariff(DataTable dtRoomTariff, long ProviderID, string MOUID, int CreatedUserRegionID, int format)
        {
            try
            {
                string result = string.Empty;
                System.IO.StringWriter sw = new System.IO.StringWriter();
                string strroomtariff;

                dtRoomTariff.TableName = "RoomTariff";
                dtRoomTariff.WriteXml(sw);
                strroomtariff = sw.ToString();

                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "USP_TariffUpload_SaveRoomTariff");
                vDBHelper.mAddParameter("@ProviderId", SqlDbType.BigInt, ParameterDirection.Input, ProviderID);
                vDBHelper.mAddParameter("@MouId", SqlDbType.BigInt, ParameterDirection.Input, MOUID);
                vDBHelper.mAddParameter("@XmlRoomTariff", SqlDbType.Xml, ParameterDirection.Input, strroomtariff);
                vDBHelper.mAddParameter("@CreatedBy", SqlDbType.Int, ParameterDirection.Input, CreatedUserRegionID);
                vDBHelper.mAddParameter("@FormatType", SqlDbType.Int, ParameterDirection.Input, format);
              //  vDBHelper.mAddParameter("@Discount", SqlDbType.Decimal, ParameterDirection.Input, Discount);

                DataTable dt = vDBHelper.ExecuteDT();

                if (dt != null && dt.Rows.Count > 0)
                {
                    result = (string)dt.Rows[0]["DataMsg"];

                }

                return result;
            }
            catch (Exception Ex)
            {
                return "Failed" + Ex.Message;
                throw;
            }

        }
        public DataTable GetFacilityTariffUpload(string StrFacilityType)
        {
            try
            {
                DataTable dt = new DataTable();
                if (StrFacilityType != string.Empty)
                {
                    vDBHelper.mCreateCommand(CommandType.Text, " select Level1, F.ID as FacilityID,Replace(Level2,'.','') Name,0 as Percentage from  Mst_Facility F where   ((ParentID=1 and Level1='Bed Strength') or Level1='" + StrFacilityType + "')" + " Order by Level2");
                }
                else
                {
                    vDBHelper.mCreateCommand(CommandType.Text, " select Level1,  F.ID as FacilityID,Replace(Level2,'.','') Name,0 as Percentage from  Mst_Facility F where ParentID=1 and Level1='Bed Strength' Order by Level2");
                }
                dt = vDBHelper.ExecuteDT();
                return dt;
            }
            catch (Exception Ex)
            {
                string errorMsg = Ex.Message;
                throw;
            }
        }
        public string UploadProcedureTariff(DataTable dtRoomTariff, DataTable dtProcTariff, DataTable dtFacilityChgs, long ProviderID, string MOUID, int CreatedUserRegionID, int format,  String MouStartDate, String MouEndDate)
        {
            try
            {
                string result = string.Empty;
                System.IO.StringWriter sw = new System.IO.StringWriter();
                string strPackagetariff = string.Empty;
                string strFacilityCharges = string.Empty;
                string strroomtariff = string.Empty;
                //dtProcTariff.DefaultView.RowFilter = " INCLUSIONS<>'' or EXCLUSIONS<>''";
                //dtProcTariff = dtProcTariff.DefaultView.ToTable();
                dtProcTariff.TableName = "PackageTariff";
                dtProcTariff.WriteXml(sw);
                strPackagetariff = sw.ToString();

                sw = new System.IO.StringWriter();
                dtFacilityChgs.TableName = "FacilityCharges";
                dtFacilityChgs.WriteXml(sw);
                strFacilityCharges = sw.ToString();

                if (dtRoomTariff != null && dtRoomTariff.Rows.Count > 0)
                {
                    sw = new System.IO.StringWriter();
                    dtRoomTariff.TableName = "RoomTariff";
                    dtRoomTariff.WriteXml(sw);
                    strroomtariff = sw.ToString();
                }
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "USP_TariffUpload_SavePackageTariff");
                vDBHelper.mAddParameter("@ProviderId", SqlDbType.BigInt, ParameterDirection.Input, ProviderID);
                vDBHelper.mAddParameter("@MouId", SqlDbType.VarChar, ParameterDirection.Input, MOUID);
                vDBHelper.mAddParameter("@XmlPackageRtMst", SqlDbType.Xml, ParameterDirection.Input, strPackagetariff);
                vDBHelper.mAddParameter("@XmlPackageRtDet", SqlDbType.Xml, ParameterDirection.Input, strFacilityCharges);
                vDBHelper.mAddParameter("@CreatedBy", SqlDbType.Int, ParameterDirection.Input, CreatedUserRegionID);
                vDBHelper.mAddParameter("@FormatType", SqlDbType.Int, ParameterDirection.Input, format);
                if (strroomtariff == string.Empty)
                {
                    vDBHelper.mAddParameter("@XmlRoomTariff", SqlDbType.Xml, ParameterDirection.Input, DBNull.Value);
                }
                else
                {
                    vDBHelper.mAddParameter("@XmlRoomTariff", SqlDbType.Xml, ParameterDirection.Input, strroomtariff);
                }
               // vDBHelper.mAddParameter("@Discount", SqlDbType.Decimal, ParameterDirection.Input, Discount);

                vDBHelper.mAddParameter("@StartDate", SqlDbType.Date, ParameterDirection.Input, MouStartDate);
                vDBHelper.mAddParameter("@EndDate", SqlDbType.Date, ParameterDirection.Input, MouEndDate);

                DataTable dt = vDBHelper.ExecuteDT();

                if (dt != null && dt.Rows.Count > 0)
                {
                    result = (string)dt.Rows[0]["DataMsg"];

                }

                return result;
            }
            catch (Exception Ex)
            {
                return "Failed" + Ex.Message;
                throw;
            }

        }
        public DataSet GetTariffPackageData(string ProviderID, string MOUID, string PkgType, string EffStartDate, string EffEndDate)
        {
            try
            {
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "Usp_TariffUpload_FillPackageData");
                vDBHelper.mAddParameter("@ProviderId", SqlDbType.BigInt, ParameterDirection.Input, ProviderID);
                vDBHelper.mAddParameter("@MouId", SqlDbType.Int, ParameterDirection.Input, MOUID);
                vDBHelper.mAddParameter("@PkgType", SqlDbType.Int, ParameterDirection.Input, PkgType);

                if (EffStartDate == string.Empty)
                {
                    vDBHelper.mAddParameter("@EffStartDate", SqlDbType.Date, ParameterDirection.Input, DBNull.Value);
                    vDBHelper.mAddParameter("@EffEndDate", SqlDbType.Date, ParameterDirection.Input, DBNull.Value);

                }
                else
                {
                    vDBHelper.mAddParameter("@EffStartDate", SqlDbType.Date, ParameterDirection.Input, Convert.ToDateTime(EffStartDate));
                    vDBHelper.mAddParameter("@EffEndDate", SqlDbType.Date, ParameterDirection.Input, Convert.ToDateTime(EffEndDate));

                }

                DataSet ds = vDBHelper.Execute();
                return ds;
            }
            catch (Exception Ex)
            {
                throw;
            }
        }


        /// <summary>
        /// Validates the input start/end dates against selected MOUs.
        /// </summary>
        /// <param name="MOUIDs">Comma-separated MOU IDs</param>
        /// <param name="inputStartDate">Input start date</param>
        /// <param name="inputEndDate">Input end date</param>
        /// <returns>Empty string if valid, else error message</returns>
        public string ValidateMOUStartEndDates(string MOUIDs, DateTime inputStartDate, DateTime inputEndDate)
        {
            try
            {
                // Split MOU IDs into list
                string[] mouIdsArray = MOUIDs.Split(',');

                // Build SQL query with IN clause to fetch all selected MOUs
                string query = $"SELECT Id, StartDate, EndDate FROM providermou WHERE Id IN ({string.Join(",", mouIdsArray)})";

                // Create command
                vDBHelper.mCreateCommand(CommandType.Text, query);

                // Execute and get DataTable
                DataSet ds = vDBHelper.Execute();
                DataTable dt = ds.Tables[0];

                // List to keep invalid MOU IDs
                StringBuilder invalidMous = new StringBuilder();

                foreach (DataRow row in dt.Rows)
                {
                    DateTime mouStart = Convert.ToDateTime(row["StartDate"]);
                    DateTime mouEnd = Convert.ToDateTime(row["EndDate"]);

                    if (inputStartDate.Date < mouStart.Date || inputEndDate.Date > mouEnd.Date)
                    {
                        invalidMous.Append(row["Id"].ToString() + ",");
                    }
                }

                if (invalidMous.Length > 0)
                {
                    // Remove trailing comma
                    string invalidIds = invalidMous.ToString().TrimEnd(',');
                    string message = $"Invalid Start/End Date for the selected MOU(s): {invalidIds}.\n" +
                                     "Please ensure that the Tariff Start Date and Tariff End Date should be in between the MOU Start Date and End Date.";
                    return message;
                }

                return string.Empty; // All dates match
            }
            catch (Exception ex)
            {
                // Optional: log exception
                throw new Exception("Error validating MOU dates", ex);
            }
        }


        public DataSet UpdateTariffPkgOverAllDisc(string _mouId, decimal _allDiscounts, long _providerIdValue, int UserRegionID, string remarks, string PkgType)
        {
            string msg = string.Empty;
            try
            {

                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "USP_TariffUpload_UpdateDiscount");
                vDBHelper.mAddParameter("@ProviderID", SqlDbType.BigInt, ParameterDirection.Input, _providerIdValue);
                vDBHelper.mAddParameter("@MOUID", SqlDbType.VarChar, ParameterDirection.Input, _mouId);
                vDBHelper.mAddParameter("@Discount", SqlDbType.Decimal, ParameterDirection.Input, _allDiscounts);
                vDBHelper.mAddParameter("@PkgType", SqlDbType.Int, ParameterDirection.Input, PkgType);

                vDBHelper.mAddParameter("@PackageDiscountRemarks", SqlDbType.VarChar, 500, ParameterDirection.Input, remarks);
                vDBHelper.mAddParameter("@ModifiedBy", SqlDbType.Int, ParameterDirection.Input, UserRegionID);
                //vDBHelper.mAddParameter("@msg", SqlDbType.VarChar, 500, ParameterDirection.Output, DBNull.Value);

                DataSet ds = vDBHelper.Execute();
                return ds;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public DataSet GetTariffPkgDisc(long ProviderID, string MOUID)
        {
            try
            {
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "USP_TariffUpload_GetPackageDiscount");
                vDBHelper.mAddParameter("@ProviderId", SqlDbType.BigInt, ParameterDirection.Input, ProviderID);
                vDBHelper.mAddParameter("@MouId", SqlDbType.VarChar, ParameterDirection.Input, MOUID);

                DataSet dsMouDisc = vDBHelper.Execute();
                return dsMouDisc;
            }
            catch (Exception Ex)
            {
                string errorMsg = Ex.Message;
                throw;
            }
        }
        //End Tariff Upload - Leena SP3V-2777 -----------------------------------------------

        #region Web Insurer Exclusion Details

        /// <summary>
        /// GetHospitalFlagAuditDetails- Getting audit details of Web Insurer Audit Details based on Provider ##SP3V-2407 ##Date:11-07-2023.
        /// </summary>
        /// <param name="ProviderID"></param>
        /// <returns>Web Insurer Audit Details as DataTable</returns>
        /// 
        public DataTable GetWebExclusionAuditDetails(int ProviderID)
        {
            DataTable dt = null;
            try
            {
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "USP_Get_AuditLog_WebExclusionDetails");
                vDBHelper.mAddParameter("@providerId", SqlDbType.BigInt, ParameterDirection.Input, ProviderID);
                dt = vDBHelper.ExecuteDT();
                return dt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// SaveWebExclusionDetails - To save the web insurer exclusion details ## 18/07/2023 ## SP3V-2400
        /// </summary>
        /// <param name="ProviderID"></param>
        /// <param name="RelevantID"></param>
        /// <param name="Remarks"></param>
        /// <param name="UserRegionID"></param>
        /// <param name="IsWebExcluded"></param>
        /// <returns></returns>
        public string SaveWebExclusionDetails(long ProviderID, long RelevantID, string Remarks, int UserRegionID, bool IsWebExcluded)
        {
            try
            {
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "USP_ProviderWebExclusionInsurer");
                vDBHelper.mAddParameter("@ProviderID", SqlDbType.BigInt, ParameterDirection.Input, ProviderID);
                vDBHelper.mAddParameter("@RelevantID", SqlDbType.BigInt, ParameterDirection.Input, RelevantID);
                vDBHelper.mAddParameter("@WebExclusionReason", SqlDbType.VarChar, 150, ParameterDirection.Input, Remarks);
                vDBHelper.mAddParameter("@CreatedUserRegionID", SqlDbType.Int, ParameterDirection.Input, UserRegionID);
                vDBHelper.mAddParameter("@IsWebExcluded", SqlDbType.Bit, ParameterDirection.Input, IsWebExcluded);
                vDBHelper.mAddParameter("@Msg", SqlDbType.VarChar, 1000, ParameterDirection.Output, "");
                string strReturn;
                vDBHelper.ExecuteNonQuery(out strReturn, "@Msg");
                return strReturn;
            }
            catch (Exception Ex)
            {
                return "Failed" + Ex.Message;
            }
        }

        /// <summary>
        /// GetWebExclusionDetails - To get the web insurer exclusion details by ProviderID ## 18/07/2023 ## SP3V-2400
        /// </summary>
        /// <param name="ProviderID"></param>
        /// <returns></returns>
        public DataTable GetWebExclusionDetails(Int32 ProviderID)
        {
            //DataSet ds = null;
            try
            {
                DataTable dt;
                vDBHelper.mCreateCommand(CommandType.Text, DBQueries.GetWebExclusiondata);
                vDBHelper.mAddParameter("@ProviderID", SqlDbType.BigInt, ParameterDirection.Input, ProviderID);
                dt = vDBHelper.ExecuteDT();
                return dt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region Web Hospital Search

        public DataTable WebHospitalSearch(string hospType, string hospCategory, string stateName, string cityName, string hospName, string prcNo)
        {
            try
            {

                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "USP_GetWebHospReport");
                vDBHelper.mAddParameter("@hospType", SqlDbType.VarChar, ParameterDirection.Input, hospType);
                vDBHelper.mAddParameter("@hospCategory", SqlDbType.VarChar, ParameterDirection.Input, hospCategory);

                if (stateName != "0")
                    vDBHelper.mAddParameter("@stateName", SqlDbType.VarChar, ParameterDirection.Input, stateName);
                if (cityName != "0")// && CityID!=null)
                    vDBHelper.mAddParameter("@cityName", SqlDbType.VarChar, ParameterDirection.Input, cityName);
                if (hospName != "null")
                    vDBHelper.mAddParameter("@hospName", SqlDbType.VarChar, ParameterDirection.Input, hospName);
                if (prcNo != "null" /*&& prcNo != null*/)
                    vDBHelper.mAddParameter("@prcNo", SqlDbType.VarChar, ParameterDirection.Input, prcNo);
                DataTable ds = vDBHelper.ExecuteDT();

                return ds;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        #endregion
        #region Insurer Wise Hospital Search

        /// <summary>
        /// InsurerwiseWebHospSearch - To get the Insurer wise Web Hospital report by ProviderID ## 18/07/2023 ## SP3V-2398
        /// </summary>
        /// <param name="hospType"></param>
        /// <param name="IssueID"></param>
        /// <param name="stateName"></param>
        /// <param name="cityName"></param>
        /// <returns></returns>

        public DataSet InsurerwiseWebHospSearch1(string hospType, string IssueID, string stateName, string cityName, string PRCNo)
        {
            vDBHelper.mCreateCommand(CommandType.StoredProcedure, "USP_InsurerWiseWebHospReport_Export");
            vDBHelper.mAddParameter("@hospType", SqlDbType.VarChar, ParameterDirection.Input, hospType);

            if (stateName != "0")
                vDBHelper.mAddParameter("@stateName", SqlDbType.VarChar, ParameterDirection.Input, stateName);
            if (cityName != "0")// && CityID!=null)
                vDBHelper.mAddParameter("@cityName", SqlDbType.VarChar, ParameterDirection.Input, cityName);
            if (IssueID != "0")
                vDBHelper.mAddParameter("@IssueID", SqlDbType.VarChar, ParameterDirection.Input, IssueID);
            if (PRCNo != "")
                vDBHelper.mAddParameter("@PRCNo", SqlDbType.VarChar, ParameterDirection.Input, PRCNo);

            DataSet dt = vDBHelper.Execute();

            return dt;
        }

        #endregion

        #region Insurer Wise Hospital Search

        /// <summary>
        /// InsurerwiseWebHospSearch - To get the Insurer wise Web Hospital report by ProviderID ## 18/07/2023 ## SP3V-2398
        /// </summary>
        /// <param name="hospType"></param>
        /// <param name="IssueID"></param>
        /// <param name="stateName"></param>
        /// <param name="cityName"></param>
        /// <returns></returns>

        public DataTable InsurerwiseWebHospSearchExportALLInsurers(string hospType, string IssueID, string stateName, string cityName, string PRCNo)
        {
            vDBHelper.mCreateCommand(CommandType.StoredProcedure, "USP_InsurerWiseWebHospReport_ExportALLInsurersData");
            vDBHelper.mAddParameter("@hospType", SqlDbType.VarChar, ParameterDirection.Input, hospType);

            if (stateName != "0")
                vDBHelper.mAddParameter("@stateName", SqlDbType.VarChar, ParameterDirection.Input, stateName);
            if (cityName != "0")// && CityID!=null)
                vDBHelper.mAddParameter("@cityName", SqlDbType.VarChar, ParameterDirection.Input, cityName);
            if (IssueID != "0")
                vDBHelper.mAddParameter("@IssueID", SqlDbType.VarChar, ParameterDirection.Input, IssueID);
            if (PRCNo != "")
                vDBHelper.mAddParameter("@PRCNo", SqlDbType.VarChar, ParameterDirection.Input, PRCNo);

            DataTable dt = vDBHelper.ExecuteDT();

            return dt;
        }

        #endregion

        #region Insurer Wise Hospital Search

        /// <summary>
        /// InsurerwiseWebHospSearch - To get the Insurer wise Web Hospital report by ProviderID ## 18/07/2023 ## SP3V-2398
        /// </summary>
        /// <param name="hospType"></param>
        /// <param name="IssueID"></param>
        /// <param name="stateName"></param>
        /// <param name="cityName"></param>
        /// <returns></returns>

        public DataTable InsurerwiseWebHospSearch(string hospType, string IssueID, string stateName, string cityName, string PRCNo)
        {
            vDBHelper.mCreateCommand(CommandType.StoredProcedure, "USP_InsurerWiseWebHospReport_Export");
            vDBHelper.mAddParameter("@hospType", SqlDbType.VarChar, ParameterDirection.Input, hospType);

            if (stateName != "0")
                vDBHelper.mAddParameter("@stateName", SqlDbType.VarChar, ParameterDirection.Input, stateName);
            if (cityName != "0")// && CityID!=null)
                vDBHelper.mAddParameter("@cityName", SqlDbType.VarChar, ParameterDirection.Input, cityName);
            if (IssueID != "0")
                vDBHelper.mAddParameter("@IssueID", SqlDbType.VarChar, ParameterDirection.Input, IssueID);
            if (PRCNo != "")
                vDBHelper.mAddParameter("@PRCNo", SqlDbType.VarChar, ParameterDirection.Input, PRCNo);

            DataTable dt = vDBHelper.ExecuteDT();

            return dt;
        }

        #endregion

        /// <summary>
        /// InsurerwiseWebHospSearch - To get the Insurer wise Web Hospital report by ProviderID ## 18/07/2023 ## SP3V-2398
        /// </summary>
        /// <param name="hospType"></param>
        /// <param name="IssueID"></param>
        /// <param name="stateName"></param>
        /// <param name="cityName"></param>
        /// <returns></returns>

        public DataTable InsurerwiseWebHospSearch(string hospType, string IssueID, string stateName, string cityName, string PRCNo, int pageNumber, int pageSize, string searchValue)
        {
            vDBHelper.mCreateCommand(CommandType.StoredProcedure, "USP_InsurerWiseWebHospReport");
            vDBHelper.mAddParameter("@hospType", SqlDbType.VarChar, ParameterDirection.Input, hospType);

            if (stateName != "0")
                vDBHelper.mAddParameter("@stateName", SqlDbType.VarChar, ParameterDirection.Input, stateName);
            if (cityName != "0")// && CityID!=null)
                vDBHelper.mAddParameter("@cityName", SqlDbType.VarChar, ParameterDirection.Input, cityName);
            if (PRCNo != "")
                vDBHelper.mAddParameter("@PRCNo", SqlDbType.VarChar, ParameterDirection.Input, PRCNo);
            if (IssueID != "0")
                vDBHelper.mAddParameter("@IssueID", SqlDbType.VarChar, ParameterDirection.Input, IssueID);
            //if (IssueID != "0")
            vDBHelper.mAddParameter("@pageNumber", SqlDbType.VarChar, ParameterDirection.Input, pageNumber);
            //if (IssueID != "0")
            vDBHelper.mAddParameter("@pageSize", SqlDbType.VarChar, ParameterDirection.Input, pageSize);
            //if (IssueID != "0")
            vDBHelper.mAddParameter("@searchValue", SqlDbType.VarChar, ParameterDirection.Input, searchValue);

            DataTable dt = vDBHelper.ExecuteDT();

            return dt;
        }



        //SP3V-3851 Leena -----------------------------------
        public DataTable GetProviderMOULog(int ProviderID, int MOUId)
        {
            try
            {
                DataTable dt = new DataTable();

                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "USP_ProviderMOULog_Details");
                vDBHelper.mAddParameter("@ProviderID", SqlDbType.BigInt, ParameterDirection.Input, ProviderID);
                vDBHelper.mAddParameter("@MOUId", SqlDbType.Int, ParameterDirection.Input, MOUId);

                dt = vDBHelper.ExecuteDT();
                return dt;
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
        }
        //End-----------------------------------------------
        //SP3V-2758 Leena
        public DataTable GetTariffDocsInfo(long providerId = 0, string MOUId = "", long TariffDocId = 0, string Type = "Mapped")
        {
            try
            {
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "Usp_TariffUploadDoc_FillDetails");
                vDBHelper.mAddParameter("@ProviderID", SqlDbType.BigInt, ParameterDirection.Input, providerId);
                vDBHelper.mAddParameter("@MOUId", SqlDbType.VarChar, ParameterDirection.Input, MOUId);

                vDBHelper.mAddParameter("@TariffDocId", SqlDbType.BigInt, ParameterDirection.Input, TariffDocId);
                vDBHelper.mAddParameter("@Type", SqlDbType.VarChar, ParameterDirection.Input, Type);
                DataTable dt = vDBHelper.ExecuteDT();
                return dt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //SP3V-4752 Leena
        public DataTable GetProviderBankDocsInfo(long providerId = 0, string MOUId = "", long TariffDocId = 0, string Type = "Mapped")
        {
            try
            {
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "Usp_GetProviderBankDocumentDetails");
                vDBHelper.mAddParameter("@ProviderID", SqlDbType.BigInt, ParameterDirection.Input, providerId);
                vDBHelper.mAddParameter("@MOUId", SqlDbType.VarChar, ParameterDirection.Input, MOUId);

                vDBHelper.mAddParameter("@TariffDocId", SqlDbType.BigInt, ParameterDirection.Input, TariffDocId);
                vDBHelper.mAddParameter("@Type", SqlDbType.VarChar, ParameterDirection.Input, Type);
                DataTable dt = vDBHelper.ExecuteDT();
                return dt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string InsertTariffDocsInfo(string file, string SystemFileName, long ProviderID, string mouid, int UserRegionID, string DocumentIds)
        {
            try
            {
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "Usp_TariffUploadDoc_Insert");

                vDBHelper.mAddParameter("@File", SqlDbType.VarChar, ParameterDirection.Input, file);
                vDBHelper.mAddParameter("@SystemFileName", SqlDbType.VarChar, ParameterDirection.Input, SystemFileName);
                vDBHelper.mAddParameter("@ProviderID", SqlDbType.BigInt, ParameterDirection.Input, ProviderID);
                vDBHelper.mAddParameter("@MOUID", SqlDbType.VarChar, ParameterDirection.Input, mouid);
                vDBHelper.mAddParameter("@CreatedUserRegionID", SqlDbType.Int, ParameterDirection.Input, UserRegionID);
                vDBHelper.mAddParameter("@DocumentIds", SqlDbType.VarChar, ParameterDirection.Input, DocumentIds);
                vDBHelper.mAddParameter("@Msg", SqlDbType.VarChar, 1000, ParameterDirection.Output, "");
                string msg;
                vDBHelper.ExecuteNonQuery(out msg, "@Msg");
                return msg;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        //public string DeleteTariffDoc(long FileId, int UserRegionID , long ProviderId, string MOUIds)
        public string DeleteTariffDoc(long FileId, int UserRegionID)
        {
            try
            {
                vDBHelper.mCreateCommand(CommandType.StoredProcedure, "Usp_TariffUploadDoc_Delete");

                vDBHelper.mAddParameter("@FileId", SqlDbType.BigInt, ParameterDirection.Input, FileId);
                vDBHelper.mAddParameter("@DeletedUserRegionID", SqlDbType.Int, ParameterDirection.Input, UserRegionID);
                vDBHelper.mAddParameter("@Msg", SqlDbType.VarChar, 1000, ParameterDirection.Output, "");

                string msg;
                vDBHelper.ExecuteNonQuery(out msg, "@Msg");
                return msg;
            }
            catch (Exception ex)
            {

                throw;
            }
        }


        //End----------------------------------------------



    }
}
