$('#ddlAdmissionType option').map(function () { return this.value + ' = ' + $(this).text(); }).get()
