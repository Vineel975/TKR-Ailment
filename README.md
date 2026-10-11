   var f = document.getElementById('form');
   console.table(Array.from(f.elements)
     .filter(function (e) { return e.willValidate && !e.checkValidity(); })
     .map(function (e) { return { field: e.name || e.id, problem: e.validationMessage, visible: !!(e.offsetWidth || e.offsetHeight) }; }));
   console.log('SAVE CODING disabled:', $('#btnCodingDetails').prop('disabled'), '| coding JSON length:', ($('#hdnClaimsCodingDetails').val() || '').length);
