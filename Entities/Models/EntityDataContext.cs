using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.Models
{
    public class EntityDataContext : DataMSContext
    {
        private readonly string _connection;

        public EntityDataContext(string connection)
        {
            _connection = connection;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(_connection);
            }
        }

        // ✅ Thêm phần này
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Ép EF hiểu rằng cột Id trong bảng User là Identity (tự tăng)
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                      .ValueGeneratedOnAdd(); // <--- Quan trọng dòng này

                // (Tuỳ chọn: nếu bạn muốn chỉ định tên bảng chính xác)
                entity.ToTable("User", "dbo");
            });
        }
    }
}
