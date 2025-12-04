using AutoMapper;
using DormitoryManagementSystem.BUS.Interfaces;
using DormitoryManagementSystem.DAO.Interfaces;
using DormitoryManagementSystem.DTO.Rooms;
using DormitoryManagementSystem.DTO.SearchCriteria; // RoomSearchCriteria
using DormitoryManagementSystem.DTO.Utils; // AppConstants
using DormitoryManagementSystem.Entity;

namespace DormitoryManagementSystem.BUS.Implementations
{
    public class RoomBUS : IRoomBUS
    {
        private readonly IRoomDAO _roomDAO;
        private readonly IBuildingDAO _buildingDAO;
        private readonly IMapper _mapper;

        public RoomBUS(IRoomDAO roomDAO, IBuildingDAO buildingDAO, IMapper mapper)
        {
            _roomDAO = roomDAO;
            _buildingDAO = buildingDAO;
            _mapper = mapper;
        }

        // ======================== GET & SEARCH ========================

        public async Task<IEnumerable<RoomReadDTO>> GetAllRoomsAsync()
        {
            // Lấy tất cả phòng Active/Maintenance (trừ Inactive)
            var list = await _roomDAO.SearchRoomsAsync(new RoomSearchCriteria { Status = null });
            // Lưu ý: DAO đã mặc định lọc != Inactive nếu status truyền vào là null
            return _mapper.Map<IEnumerable<RoomReadDTO>>(list);
        }

        public async Task<IEnumerable<RoomReadDTO>> GetAllRoomsIncludingInactivesAsync() =>
            _mapper.Map<IEnumerable<RoomReadDTO>>(await _roomDAO.GetAllRoomsIncludingInactivesAsync());

        public async Task<RoomReadDTO?> GetRoomByIDAsync(string id)
        {
            var room = await _roomDAO.GetRoomByIDAsync(id);
            return room == null ? null : _mapper.Map<RoomReadDTO>(room);
        }

        public async Task<RoomDetailDTO?> GetRoomDetailByIDAsync(string id)
        {
            var r = await _roomDAO.GetRoomDetailByIDAsync(id);
            if (r == null) return null;

            return new RoomDetailDTO
            {
                RoomID = r.Roomid,
                RoomNumber = r.Roomnumber,
                BuildingName = r.Building?.Buildingname ?? "Unknown",
                Capacity = r.Capacity,
                CurrentOccupancy = r.Currentoccupancy ?? 0,
                Price = r.Price,
                Status = r.Status ?? "Unknown",
                AllowCooking = r.Allowcooking ?? false,
                AirConditioner = r.Airconditioner ?? false
            };
        }

        public async Task<IEnumerable<RoomReadDTO>> GetRoomsByBuildingIDAsync(string buildingId)
        {
            if (await _buildingDAO.GetByIDAsync(buildingId) == null)
                throw new KeyNotFoundException($"Tòa nhà {buildingId} không tồn tại.");

            var list = await _roomDAO.SearchRoomsAsync(new RoomSearchCriteria { BuildingID = buildingId });
            return _mapper.Map<IEnumerable<RoomReadDTO>>(list);
        }

        // Admin Search Keyword
        public async Task<IEnumerable<RoomReadDTO>> SearchRoomsAsync(string keyword)
        {
            var list = await _roomDAO.SearchRoomsAsync(new RoomSearchCriteria { Keyword = keyword });
            return _mapper.Map<IEnumerable<RoomReadDTO>>(list);
        }

        // Student Search (Card View)
        public async Task<IEnumerable<RoomDetailDTO>> SearchRoomInCardAsync(
            string? bId, int? num, int? cap, decimal? min, decimal? max, bool? cook, bool? ac)
        {
            var criteria = new RoomSearchCriteria
            {
                BuildingID = bId,
                RoomNumber = num,
                Capacity = cap,
                MinPrice = min,
                MaxPrice = max,
                AllowCooking = cook,
                AirConditioner = ac,
                Status = AppConstants.RoomStatus.Active // Chỉ hiện phòng Active cho SV
            };

            var rooms = await _roomDAO.SearchRoomsAsync(criteria);

            // Logic nghiệp vụ: Chỉ lấy phòng còn chỗ (Occupancy < Capacity)
            var availableRooms = rooms.Where(r => r.Currentoccupancy < r.Capacity);

            return availableRooms.Select(r => new RoomDetailDTO
            {
                RoomID = r.Roomid,
                RoomNumber = r.Roomnumber,
                BuildingName = r.Building?.Buildingname ?? "",
                Capacity = r.Capacity,
                CurrentOccupancy = r.Currentoccupancy ?? 0,
                Price = r.Price,
                Status = r.Status ?? "",
                AllowCooking = r.Allowcooking ?? false,
                AirConditioner = r.Airconditioner ?? false
            });
        }

        // Student Search (Grid View)
        public async Task<IEnumerable<RoomGridDTO>> SearchRoomInGridAsync(
             string? bId, int? num, int? cap, decimal? min, decimal? max)
        {
            var criteria = new RoomSearchCriteria
            {
                BuildingID = bId,
                RoomNumber = num,
                Capacity = cap,
                MinPrice = min,
                MaxPrice = max,
                Status = AppConstants.RoomStatus.Active
            };

            var rooms = await _roomDAO.SearchRoomsAsync(criteria);
            var availableRooms = rooms.Where(r => r.Currentoccupancy < r.Capacity);

            return availableRooms.Select(r => new RoomGridDTO
            {
                RoomID = r.Roomid,
                RoomNumber = r.Roomnumber,
                BuildingName = r.Building?.Buildingname ?? "",
                Capacity = r.Capacity,
                CurrentOccupancy = r.Currentoccupancy ?? 0,
                Status = r.Status ?? ""
            });
        }

        public async Task<IEnumerable<int>> GetRoomCapacitiesAsync() => await _roomDAO.GetDistinctCapacitiesAsync();

        public IEnumerable<RoomPriceDTO> GetPriceRanges() => new List<RoomPriceDTO>
        {
            new() { DisplayText = "Tất cả", MinPrice = null, MaxPrice = null },
            new() { DisplayText = "Dưới 1 triệu", MinPrice = 0, MaxPrice = 1000000 },
            new() { DisplayText = "1 - 2 triệu", MinPrice = 1000000, MaxPrice = 2000000 },
            new() { DisplayText = "2 - 3 triệu", MinPrice = 2000000, MaxPrice = 3000000 },
            new() { DisplayText = "Trên 3 triệu", MinPrice = 3000000, MaxPrice = null }
        };

        // ======================== TRANSACTIONS ========================

        public async Task<string> AddRoomAsync(RoomCreateDTO dto)
        {
            if (await _roomDAO.GetRoomByIDAsync(dto.RoomID) != null)
                throw new InvalidOperationException($"Phòng {dto.RoomID} đã tồn tại.");

            if (await _buildingDAO.GetByIDAsync(dto.BuildingID) == null)
                throw new KeyNotFoundException($"Tòa nhà {dto.BuildingID} không tồn tại.");

            var roomEntity = _mapper.Map<Room>(dto);
            roomEntity.Currentoccupancy = 0;

            await _roomDAO.AddRoomAsync(roomEntity);
            return roomEntity.Roomid;
        }

        public async Task UpdateRoomAsync(string id, RoomUpdateDTO dto)
        {
            var roomEntity = await _roomDAO.GetRoomByIDAsync(id)
                             ?? throw new KeyNotFoundException($"Phòng {id} không tồn tại.");

            if (dto.BuildingID != roomEntity.Buildingid)
            {
                if (await _buildingDAO.GetByIDAsync(dto.BuildingID) == null)
                    throw new KeyNotFoundException($"Tòa nhà {dto.BuildingID} không tồn tại.");
            }

            if (dto.Capacity < roomEntity.Currentoccupancy)
                throw new InvalidOperationException($"Sức chứa mới ({dto.Capacity}) nhỏ hơn số sinh viên hiện tại ({roomEntity.Currentoccupancy}).");

            _mapper.Map(dto, roomEntity);
            roomEntity.Roomid = id;

            await _roomDAO.UpdateRoomAsync(roomEntity);
        }

        public async Task DeleteRoomAsync(string id)
        {
            var roomEntity = await _roomDAO.GetRoomByIDAsync(id)
                             ?? throw new KeyNotFoundException($"Phòng {id} không tồn tại.");

            if (roomEntity.Currentoccupancy > 0)
                throw new InvalidOperationException($"Không thể xóa phòng {id} vì đang có sinh viên.");

            await _roomDAO.DeleteRoomAsync(id);
        }
    }
}