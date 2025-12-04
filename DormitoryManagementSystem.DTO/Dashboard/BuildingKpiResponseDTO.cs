using System.Collections.Generic;

namespace DormitoryManagementSystem.DTO.Dashboard
{
    public class BuildingKpiResponseDTO
    {
        public List<BuildingKpiDTO> Buildings { get; set; } = new();
    }
}