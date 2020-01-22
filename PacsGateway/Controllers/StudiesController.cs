using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace PacsGateway.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudiesController : ControllerBase
    {
        readonly DicomService m_DicomService;

        public StudiesController(DicomService dicomService)
        {
            m_DicomService = dicomService;
        }

        [HttpGet("{id}/open")]
        public async Task<ActionResult<int>> Open(string id, bool move = false)
        {
            try
            {
                var seriesCount = await m_DicomService.GetSeriesCount(id);
                if (seriesCount > 0)
                {
                    await m_DicomService.OpenStudy(id, move);
                    return seriesCount;
                }
                else
                {
                    return NotFound();
                }
            }
            catch
            {
                return StatusCode(500);
            }
        }
    }
}
