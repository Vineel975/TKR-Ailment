// Discharge date — the reliable source is the RAW Hospitalization Details
// value data[0].DateofDischarge, held in the global DOD and formatted with
// JSONDate2 (the same formatting the #txtHospDOD field uses). The field
// itself can be blank at read time OR overwritten with the admission date by
// the day-care datepicker (ServiceTypeID==2), which made the discharge match
// the admission. So read the raw value first; fall back to the field only if
// the raw value is unavailable.
(function () {
    var _dischargeSet = false;
    try {
        if (typeof DOD !== 'undefined' && DOD && typeof JSONDate2 === 'function') {
            var _fmt = JSONDate2(DOD);
            if (_fmt) { spectraFields.dischargeDate = _fmt.toString().trim(); _dischargeSet = true; }
        }
    } catch (e) { /* fall through to the field */ }
    if (!_dischargeSet) {
        var _dodFld = getInputVal('txtHospDOD');
        if (_dodFld) spectraFields.dischargeDate = _dodFld.toString().trim();
    }
})();
