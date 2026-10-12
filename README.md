### 1. Delay in Claim Creation and Process Button Availability

**Issue:**  
The page load time for documents to be uploaded and appear against the claim number takes approximately 3–5 minutes. The Process button also takes an additional 3–5 minutes to become visible, resulting in a total wait time of up to 10 minutes before the claim can be processed.

**Response:**  
During claim creation, the claim number is generated before all uploaded documents are successfully stored in S3. If the scheduler picks up the claim for AI extraction before the document upload is completed, the extraction process fails with a **"Medical Bill Not Found"** error. To prevent this scenario, the scheduler pickup time has been increased so that claims are processed only after sufficient time has elapsed for document uploads to complete.

---

### 2. Claim Details Page Keeps Loading When Multiple Tabs Are Open

**Issue:**  
When a user creates a claim in one browser tab and simultaneously attempts to open another claim by clicking the Process button in a different tab, the claim details page continues loading until the claim number appears in the claim creation tab.

**Response:**  
This behavior affects only the tabs belonging to the same user session. Other users operate within separate sessions and are not impacted. The issue does not cause application crashes or affect other users' ability to access and process claims.

---

### 3. "Back to Dashboard" Button Not Working

**Issue:**  
The "Back to Dashboard" functionality is not working as expected.

**Response:**  
No changes related to the "Back to Dashboard" functionality were made as part of our implementation. During debugging, an error stating **"Redis is not connected"** was observed during application startup.If any config keys needs to be added to mitigate this issue, pls specify.
