using AutoMapper;
using DormitoryManagementSystem.BUS.Interfaces;
using DormitoryManagementSystem.DAO.Interfaces;
using DormitoryManagementSystem.DTO.News;
using DormitoryManagementSystem.Entity;
using DormitoryManagementSystem.Utils; // Using AppConstants

namespace DormitoryManagementSystem.BUS.Implementations
{
    public class NewsBUS : INewsBUS
    {
        private readonly INewsDAO _newsDAO;
        private readonly IUserDAO _userDAO;
        private readonly IMapper _mapper;

        public NewsBUS(INewsDAO newsDAO, IUserDAO userDAO, IMapper mapper)
        {
            _newsDAO = newsDAO;
            _userDAO = userDAO;
            _mapper = mapper;
        }

        public async Task<IEnumerable<NewsReadDTO>> GetAllNewsAsync() =>
            _mapper.Map<IEnumerable<NewsReadDTO>>(await _newsDAO.GetAllNewsAsync());

        public async Task<IEnumerable<NewsReadDTO>> GetAllNewsIncludingInactivesAsync() =>
            _mapper.Map<IEnumerable<NewsReadDTO>>(await _newsDAO.GetAllNewsIncludingInactivesAsync());

        public async Task<NewsReadDTO?> GetNewsByIDAsync(string id)
        {
            var news = await _newsDAO.GetNewsByIDAsync(id);
            return news == null ? null : _mapper.Map<NewsReadDTO>(news);
        }

        public async Task<string> AddNewsAsync(NewsCreateDTO dto)
        {
            if (await _newsDAO.GetNewsByIDAsync(dto.NewsID) != null)
                throw new InvalidOperationException($"News ID {dto.NewsID} đã tồn tại.");

            if (!string.IsNullOrEmpty(dto.AuthorID))
            {
                var author = await _userDAO.GetUserByIDAsync(dto.AuthorID)
                             ?? throw new KeyNotFoundException($"Tác giả {dto.AuthorID} không tồn tại.");

                if (!author.IsActive) throw new InvalidOperationException("Tài khoản tác giả không hoạt động.");
                if (author.Role != AppConstants.Role.Admin) throw new UnauthorizedAccessException("Chỉ Admin mới được đăng tin.");
            }

            var news = _mapper.Map<News>(dto);
            if (news.Publisheddate == null) news.Publisheddate = DateTime.Now;

            await _newsDAO.AddNewsAsync(news);
            return news.Newsid;
        }

        public async Task UpdateNewsAsync(string id, NewsUpdateDTO dto)
        {
            var news = await _newsDAO.GetNewsByIDAsync(id)
                       ?? throw new KeyNotFoundException($"News {id} không tồn tại.");

            _mapper.Map(dto, news);
            news.Newsid = id;
            await _newsDAO.UpdateNewsAsync(news);
        }

        public async Task DeleteNewsAsync(string id)
        {
            if (await _newsDAO.GetNewsByIDAsync(id) == null)
                throw new KeyNotFoundException($"News {id} không tồn tại.");
            await _newsDAO.DeleteNewsAsync(id);
        }

        public async Task<IEnumerable<NewsSummaryDTO>> GetNewsSummariesAsync()
        {
            var list = await _newsDAO.GetNewsSummariesAsync();
            return list.Select(n => new NewsSummaryDTO { NewsID = n.Newsid, Title = n.Title, PublishedDate = n.Publisheddate ?? DateTime.MinValue });
        }
    }
}