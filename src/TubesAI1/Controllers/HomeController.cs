﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using TubesAI1.Scheduling;

namespace TubesAI1.Controllers
{
    public class HomeController : Controller
    {
        private IHostingEnvironment _environment;

        public HomeController(IHostingEnvironment environment)
        {
            _environment = environment;
        }

        public IActionResult Index()
        {
            return View();
        }
        
        public IActionResult Error()
        {
            ViewData["Message"] = "An error has occured.";
            return View();
        }

        private int timeToInt (string s)
        {
            // Asumsi hanya ada 5 karakter pada s
            var sr = new StringReader(s);
            int temp;
            string buf = "";
            char b;
            b = (char)sr.Read();
            buf += b;
            b = (char)sr.Read();
            buf += b;
            temp = Int32.Parse(buf);

            return temp;
        }

        [HttpPost]
        public async Task<IActionResult> Index(ICollection<IFormFile> files)
        {
            var uploads = Path.Combine(_environment.WebRootPath, "uploads");
            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    uploads = Path.Combine(uploads, file.FileName);
                    using (var fileStream = new FileStream(uploads, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }
                }
            }
            var openFile = new FileStream(uploads, FileMode.Open, FileAccess.Read);
            using (var readStream = new StreamReader(openFile))
            {
                ViewData["Message"] = "";
                string line;

                Boolean isRuangan = false;
                Boolean isKelas = false;
                string temp = "";
                char b;
                RuanganManagement listOfRuangan = new RuanganManagement();
                KelasManagement listOfKelas = new KelasManagement();
                
                /* ADT RUANGAN */
                string nama = "";                // Contoh: "7602"
                int jam_buka = 0;               // Contoh: 7 (buka mulai jam 7)
                int jam_tutup = 0;              // Contoh: 14 (tutup jam 14)
                List<int> hari_buka = new List<int>();

                /* ADT KELAS */
                // Domain value yang dimiliki 'Kelas'
                string ruangan = "";
                List<Ruangan> domainRuangan = new List<Ruangan>();
                List<int> domainMulai = new List<int>();          // Contoh: [7, 8, 9] (Kelas hanya bisa mulai jam 7, 8, atau 9)
                List<int> domainHari = new List<int>();           // Contoh: [1, 3, 5] (Kelas hanya bisa dilakukan hari Senin, Rabu, atau Jumat)
                int durasi = 0;                     // Contoh: 4 (4 jam)


                while ((line = readStream.ReadLine()) != null)
                {
                    if (line.Equals(""))
                    {
                        // Do nothing
                    }
                    else if (line.Equals("Ruangan"))
                    {
                        isRuangan = true;
                        isKelas = false;
                    }
                    else if (line.Equals("Jadwal"))
                    {
                        isRuangan = false;
                        isKelas = true;
                    }
                    else if (isRuangan)
                    {
                        var sr = new StringReader(line);
                        int point = 0;
                        temp = "";
                        hari_buka = new List<int>();
                        domainMulai = new List<int>();
                        domainRuangan = new List<Ruangan>();
                        for (int i = 0; i < line.Length; i++)
                        {
                            b = (char)sr.Read();
                            if (b == ';')
                            {
                                if (point == 0)
                                {
                                    nama = temp;
                                    ViewData["Test"] += temp + " ";
                                }
                                else if (point == 1)
                                {
                                    jam_buka = timeToInt(temp);
                                }
                                else if (point == 2)
                                {
                                    jam_tutup = timeToInt(temp);
                                }
                                point++;
                                temp = "";
                            }
                            else
                            {
                                if (b != ',') temp += b;
                            }
                        }
                        var parse = new StringReader(temp);
                        for (int j = 0; j < temp.Length; j++)
                        {
                            int val = parse.Read();
                            hari_buka.Add(val);
                        }
                        listOfRuangan.addRuangan(new Ruangan(nama, jam_buka, jam_tutup, hari_buka));
                    }
                    else if (isKelas)
                    {
                        var sr = new StringReader(line);
                        int point = 0;
                        temp = "";
                        hari_buka = new List<int>();
                        for (int i = 0; i < line.Length; i++)
                        {
                            b = (char)sr.Read();
                            if (b == ';')
                            {
                                if (point == 0)
                                {
                                    nama = temp;
                                }
                                else if (point == 1)
                                {
                                    if (!temp.Equals("-"))
                                    {
                                        ruangan = temp;
                                    }
                                }
                                else if (point == 2)
                                {
                                    jam_buka = timeToInt(temp);
                                } else if (point == 3)
                                {
                                    jam_tutup = timeToInt(temp);
                                } else if (point == 4)
                                {
                                    durasi = Int32.Parse(temp);
                                }
                                point++;
                                temp = "";
                            }
                            else
                            {
                                if (b != ',') temp += b;
                            }
                        }
                        var parse = new StringReader(temp);
                        for (int j = 0; j < temp.Length; j++)
                        {
                            int val = parse.Read();
                            hari_buka.Add(val);
                        }
                        listOfKelas.addKelas(new Kelas(nama, ruangan, jam_buka, jam_tutup, durasi, hari_buka, listOfRuangan));
                    }
                }
                ViewData["Length"] = listOfKelas;
                ViewData["Test"] = listOfRuangan;
            }
            return View();
        }
    }
}
