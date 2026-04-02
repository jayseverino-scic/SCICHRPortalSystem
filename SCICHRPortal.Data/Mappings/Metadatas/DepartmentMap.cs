using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SCICHRPortal.Data.Entities.Metadatas;

namespace SCICHRPortal.Data.Mappings.Metadatas
{
    public class DepartmentMap
    {
        public DepartmentMap(EntityTypeBuilder<Department> entityBuilder)
        {
            entityBuilder.HasKey(d => d.DepartmentId);
            entityBuilder.Property(d => d.DeptCode).HasMaxLength(10).IsRequired();
            entityBuilder.Property(d => d.DepartmentName).HasMaxLength(100).IsRequired();
            //entityBuilder.HasData(new Department[] {
            //    new Department
            //    {
            //        DepartmentId = 1,
            //        DeptCode = "Fac",
            //        DepartmentName = "Faculty"
            //    },
            //    new Department {
            //        DepartmentId = 2,
            //        DeptCode = "Admin",
            //        DepartmentName = "Administration"
            //    },
            //    new Department {
            //        DepartmentId = 3,
            //        DeptCode = "Jan",
            //        DepartmentName = "Janitorial"
            //    },
            //    new Department {
            //        DepartmentId = 4,
            //        DeptCode = "Main",
            //        DepartmentName = "Maintenance"
            //    },
            //    new Department {
            //        DepartmentId = 5,
            //        DeptCode = "Acctg",
            //        DepartmentName = "Accounting"
            //    },
            //    new Department {
            //        DepartmentId = 6,
            //        DeptCode = "Sec",
            //        DepartmentName = "Security"
            //    },
            //    new Department {
            //        DepartmentId = 7,
            //        DeptCode = "Mgmt",
            //        DepartmentName = "Management"
            //    }
            //}); 
        }
    }
}
