   (function () {
     var c = document.getElementById('claimAiConfig');
     var f = document.getElementById('ifrClaimAI');
     var tok = c ? (c.getAttribute('data-context-token') || '') : null;
     console.log('1. #claimAiConfig present :', !!c);
     console.log('2. data-context-token     :', tok === null ? 'n/a' : (c.hasAttribute('data-context-token') ? 'attribute present, length ' + tok.length : 'ATTRIBUTE MISSING'));
     console.log('3. script version         :', [].slice.call(document.scripts).map(function (s) { return s.src; }).filter(function (s) { return /ClaimAIIntegration/.test(s); }).join(', ') || 'NOT LOADED');
     console.log('4. contextParam function  :', typeof _claimAI_contextParam === 'function' ? _claimAI_contextParam.toString().indexOf('ctx') > -1 ? 'new (ctx)' : 'OLD (roleId/regionId)' : 'not defined');
     console.log('5. iframe src             :', f ? f.getAttribute('src') : 'no iframe');
   })();
