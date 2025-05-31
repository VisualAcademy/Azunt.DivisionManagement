﻿using Microsoft.EntityFrameworkCore;

namespace Azunt.DivisionManagement
{
    /// <summary>
    /// DivisionApp에서 사용하는 데이터베이스 컨텍스트 클래스입니다.
    /// Entity Framework Core와 데이터베이스 간의 브리지 역할을 합니다.
    /// </summary>
    public class DivisionDbContext : DbContext
    {
        /// <summary>
        /// DbContextOptions을 인자로 받는 생성자입니다.
        /// 주로 Program.cs 또는 Startup.cs에서 서비스로 등록할 때 사용됩니다.
        /// </summary>
        public DivisionDbContext(DbContextOptions<DivisionDbContext> options)
            : base(options)
        {
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        /// <summary>
        /// 데이터베이스 모델을 설정하는 메서드입니다.
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Divisions 테이블의 CreatedAt 열은 기본값으로 현재 날짜/시간을 사용합니다.
            modelBuilder.Entity<Division>()
                .Property(m => m.CreatedAt)
                .HasDefaultValueSql("GetDate()");
        }

        /// <summary>
        /// DivisionApp 관련 테이블을 정의합니다.
        /// </summary>
        public DbSet<Division> Divisions { get; set; } = null!;
    }
}