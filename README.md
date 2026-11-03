   SELECT ur.ID AS UserRegionID, ur.RegionID, ur.UserID, u.Name, u.LoginID,
          u.DefaultRoleID, u.AuthorizedMaxAmount
   FROM dbo.Lnk_UserRegions ur WITH (NOLOCK)
   JOIN dbo.Mst_Users u WITH (NOLOCK) ON u.ID = ur.UserID
   WHERE ur.ID = 11645;
