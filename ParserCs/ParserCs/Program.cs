using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Xml;
using System.Xml.Linq;
using HtmlAgilityPack;
using Aspose.Words;
using Aspose.Words.Saving;
using Npgsql;
using System.Management.Automation.Language;

public class Program
{
    struct Job
    {
        string when;
        string where;
        string who;

        public Job(string when = "", string where = "", string who = "")
        {
            this.when = when;
            this.where = where;
            this.who = who;
        }
        public string GetInfo()
        { return ($"{this.when} / {this.where} / {this.who}"); }
    }
    struct Education
    {
        string when;
        string where;
        string who;

        public Education(string when = "", string where = "", string who = "")
        {
            this.when = when;
            this.where = where;
            this.who = who;
        }
        public string GetInfo()
        { return ($"{this.when} / {this.where} / {this.who}"); }

    }
    struct Qualification
    {
        string when;
        string where;
        string what;

        public Qualification(string when = "", string where = "", string what = "")
        {
            this.when = when;
            this.where = where;
            this.what = what;
        }
        public string GetInfo()
        { return ($"{this.when} / {this.where} / {this.what}"); }
    }
    struct ElemsAfterDoc
    {
        public string text;
        public string styleP;
        public string styleSpan;
        public bool header;

        public ElemsAfterDoc(string text, string styleP, string styleSpan, bool header)
        {
            this.text = text;
            this.styleP = styleP;
            this.styleSpan = styleSpan;
            this.header = header;
        }
        public string GetInfo(bool withStyles)
        {
            if (withStyles)
            {
                if (this.header)
                    return ($"================ {this.text} / {this.styleP} / {this.styleSpan} ================");

                return ($"{this.text} / {this.styleP} / {this.styleSpan}");
            }

            if (this.header)
                return ($"================ {this.text} ================");

            return ($"{this.text}");
        }
    }


    static void ParseResume(string url)
    {//тут выводится инфа из резюме и сохраняется в переменные
        int id_pers_inf = 1;// id в бд персональной информации
        int id_edu = 1;// id для связи с бд образование
        int id_skil = 1;// id для связи бд  навыки  
        int id_job = 1;//  id для связи c бд должности прошлой работы
        int id_emp = 1;// id ля связи с бд занятость 
        int id_res = 1;//id для связи с бд резюме
        int id_sch = 1;
        int id_lw = 1;
        var cs = "Host=localhost;Port=5433;Username=postgres;Password=369147258mM;Database=postgres";
        using var con = new NpgsqlConnection(cs);
        con.Open();
        Console.WriteLine("=====================================Парсинг резюме....\n");

        string urlResume = url;//

        HtmlWeb webResume = new HtmlWeb();
        HtmlDocument docResume = webResume.Load(urlResume);
        /*HtmlDocument docResume = new HtmlDocument();
        docResume.Load("../../../Document1.html");*/


        string personalName = "";//имя
        string sex = ""; //пол
        string age = "";
        string birthday = "";
        string phoneNumber = ""; //телефонный номер
        string mail = ""; //почта
        string city = "";
        //ГОТОВНОСТЬ К ПЕРЕЕЗДУ

        string position = ""; //позиция
        List<string> specs = new List<string>(); //специализации
        string busyness = "";//ЗАНЯТОСТЬ
        string workSchedule = "";//ГРАФИК РАБОТЫ
        string salary = "зарплата не указана"; //Зарплата

        string workExpTime = " ";// опыт работы по времени
        List<Job> jobs = new List<Job>();// работы

        List<string> skills = new List<string>(); //ключевые навыки

        string driverExp = ""; //опыт вождения (категории)

        string aboutMe = "";

        List<Education> educations = new List<Education>();

        List<string> languages = new List<string>(); //ключевые навыки

        List<Qualification> qualifications = new List<Qualification>();

        string citizenship = "";

        var personAndContacts = docResume.DocumentNode.SelectSingleNode("//div[@class='resume-header']"); //блок с именем и контактной информацией
        if (personAndContacts == null)
        {
            /*Console.WriteLine("Не указана персональная информация");
            Console.WriteLine();*/
        }
        else
        {
            var personalElemHTML = personAndContacts.SelectSingleNode(".//h2[@data-qa='resume-personal-name']");
            if (personalElemHTML != null)
            {
                personalName = personalElemHTML.InnerText;
                Console.WriteLine("Имя: " + personalName);
            }

            personalElemHTML = personAndContacts.SelectSingleNode(".//span[@data-qa='resume-personal-gender']");
            if (personalElemHTML != null)
            {
                sex = personalElemHTML.InnerText;
                Console.WriteLine("Пол: " + sex);
            }

            personalElemHTML = personAndContacts.SelectSingleNode(".//span[@data-qa='resume-personal-age']");
            if (personalElemHTML != null)
            {
                age = personalElemHTML.InnerText;
                Console.WriteLine("Возраст: " + age);//resume-personal-birthday
            }

            personalElemHTML = personAndContacts.SelectSingleNode(".//span[@data-qa='resume-personal-birthday']");
            if (personalElemHTML != null)
            {
                birthday = personalElemHTML.InnerText;
                Console.WriteLine("Дата рождения: " + birthday);
            }

            personalElemHTML = personAndContacts.SelectSingleNode(".//div[@data-qa='resume-contacts-phone']/span");
            if (personalElemHTML != null)
            {
                phoneNumber = personalElemHTML.InnerText;
                Console.WriteLine("Номер телефона: " + phoneNumber);
            }

            personalElemHTML = personAndContacts.SelectSingleNode(".//div[@data-qa='resume-contact-email']/a");
            if (personalElemHTML != null)
            {
                mail = personalElemHTML.InnerText;
                Console.WriteLine("Электронная почта: " + mail);
            }

            personalElemHTML = personAndContacts.SelectSingleNode(".//span[@data-qa='resume-personal-address']");
            if (personalElemHTML != null)
            {
                city = personalElemHTML.InnerText;
                Console.WriteLine("Город: " + city);
            }

            //ГОТОВНОСТЬ К ПЕРЕЕЗДУ

            Console.WriteLine();

        }
        var citizenship1 = docResume.DocumentNode.SelectSingleNode("//div[@data-qa='resume-block-additional']/div[@class='resume-block-item-gap']"); //блок "Гражданство, время в пути до работы"
        if (citizenship1 == null)
        {
            /*Console.WriteLine("Нет информации о гражданстве, разрешения на работу и желательного времени до работы");
            Console.WriteLine();*/
        }
        else
        {
            var citizenship1Container = citizenship1.SelectNodes(".//p");
            foreach (var info in citizenship1Container)
            {
                string text = info.InnerText;
                if (text.Contains("Гражданство: "))
                {
                    citizenship = text.Replace("Гражданство: ", "");
                    break;
                }
            }
            Console.WriteLine("Гражданство:\n    " + citizenship);
            String[] words;
            words = personalName.Split(new char[] { ' ' });
            string a;
            if (words.Length == 3)
            {
                a = words[2];
            }
            else
            {
                a = " ";
            }
            if (words.Length == 1)
            {
                a = "";
                if(age == "")
                {
                    age = "0";
                }
                age = age.Trim(new char[] { 'г', 'о', 'д', ' ','л','е','т','а' }); // результат "ello worl"
                int vozr = int.Parse(age);
                birthday = birthday.Replace(" ", "");
                id_pers_inf = personal_information_poisk(a, a, a, vozr, sex, mail, phoneNumber, birthday, citizenship, city, con);
                if (id_pers_inf == 0)
                {
                    personal_information(a, a, a, vozr, sex, mail, phoneNumber, birthday, citizenship, city, con);
                    id_pers_inf = personal_information_poisk(a, a, a, vozr, sex, mail, phoneNumber, birthday, citizenship, city, con);
                }
            }
            else
            {
                age = age.Trim(new char[] { 'г', 'о', 'д', ' ', 'л', 'е', 'т','а' }); // результат "ello worl"
                int vozr = int.Parse(age);
                birthday = birthday.Replace(" ", "");
                id_pers_inf = personal_information_poisk(words[0], words[1], a, vozr, sex, mail, phoneNumber, birthday, citizenship, city, con);
                if (id_pers_inf == 0)
                {
                    personal_information(words[0], words[1], a, vozr, sex, mail, phoneNumber, birthday, citizenship, city, con);
                    id_pers_inf = personal_information_poisk(words[0], words[1], a, vozr, sex, mail, phoneNumber, birthday, citizenship, city, con);
                }
            }


            Console.WriteLine();

        }
        Console.WriteLine("===================================================================");

        var resumePos = docResume.DocumentNode.SelectSingleNode("//div[@data-qa='resume-block-position']"); //блок с инфой про позицию
        if (resumePos == null)
        {
            /*Console.WriteLine("Не указана информация о позиции");
            Console.WriteLine();*/
        }
        else
        {
            var personalElemHTML = resumePos.SelectSingleNode(".//span[@class='resume-block__title-text']");
            if (personalElemHTML != null)
            {
                position = personalElemHTML.InnerText;
                Console.WriteLine("Позиция: " + position);
                id_job = jobtitle_poisk(position, con);
                if (id_job == 0)
                {
                    job_title(position, con);
                    id_job = jobtitle_poisk(position, con);
                }



            }

            var personalElemHTMLs1 = resumePos.SelectNodes(".//li[@class='resume-block__specialization']");
            //if (personalElemHTMLs1 != null)
            //{
            //    foreach (var special in personalElemHTMLs1)
            //    {
            //        specs.Add(special.InnerText);
            //    }

            //    Console.WriteLine("Специализации:");
            //    foreach (string spec in specs)
            //    {
            //        Console.WriteLine("    " + spec);
            //    }

            //}

            personalElemHTMLs1 = resumePos.SelectNodes(".//p");
            if (personalElemHTMLs1 != null)
            {
                foreach (var special in personalElemHTMLs1)
                {
                    string text = special.InnerText;
                    if (text.Contains("Занятость: "))
                    {
                        busyness = text.Replace("Занятость: ", "");
                        Console.WriteLine("    Занятость: " + busyness);
                        id_emp = employment_poisk(busyness, con);
                        if (id_emp == 0)
                        {
                            Employment(busyness, con);
                            id_emp = employment_poisk(busyness, con);
                        }
                    }
                    else if (text.Contains("График работы: "))
                    {
                        workSchedule = text.Replace("График работы: ", "");
                        Console.WriteLine("    График работы: " + workSchedule);
                        id_sch = schedule_poisk(workSchedule, con);
                        if (id_sch == 0)
                        {
                            schedule(workSchedule, con);
                            id_sch = schedule_poisk(workSchedule, con);
                        }

                    }
                }
            }

            //ЗАНЯТОСТЬ ДОБАВИЛ
            //ГРАФИК РАБОТЫ ДОБАВИЛ

            personalElemHTML = resumePos.SelectSingleNode(".//div[@class='resume-block-position-salary']");//зарплата
            if (personalElemHTML != null)
            {
                salary = personalElemHTML.InnerText;
            }
            Console.WriteLine("    Зарплата: " + salary);
            Console.WriteLine();

        }



        var skillsTable = docResume.DocumentNode.SelectSingleNode("//div[@data-qa='skills-table']"); //блок с ключевыми навыками
        if (skillsTable == null)
        {
            /* Console.WriteLine("Нет ключевых навыков");
             Console.WriteLine();*/
        }
        else
        {
            var skillsHTML = skillsTable.SelectNodes(".//span[@class='bloko-tag__section bloko-tag__section_text']");
            Console.WriteLine("Ключевые навыки: ");
            foreach (var skill in skillsHTML)
            {
                skills.Add(skill.InnerText);
                id_skil = skills_poisk(skill.InnerText, con);
                if (id_skil == 0)
                {
                    skilll(skill.InnerText, con);
                    id_skil = skills_poisk(skill.InnerText, con);
                }

                infskill(id_pers_inf, id_skil, con);
            }
            skills.ForEach(i => Console.WriteLine("    {0}", i));


            Console.WriteLine();

        }

        var about = docResume.DocumentNode.SelectSingleNode("//div[@data-qa='resume-block-skills']/div[@class='resume-block-item-gap']"); //блок "Обо мне"
        if (about == null)
        {
            /*Console.WriteLine("Нет информации о себе");
            Console.WriteLine();*/
        }
        else
        {
            aboutMe = about.InnerText;
            Console.WriteLine("Обо мне:\n    " + aboutMe);

            String[] words;
            string substring = "зарплата не указана";
            int indexOfSubstring = salary.IndexOf(substring);
            if (indexOfSubstring == -1)
            {
                words = salary.Split(new char[] { '₽', 'р', 'у', 'б', '.' });

                salary = words[0].Trim(new char[] { ' ', ' ', });
                salary = salary.Replace(" ", "");

                int z = int.Parse(salary);
            }
            else
            {
                salary = "0";
            }
          
            id_res = resume_poisk(urlResume, workExpTime, aboutMe, int.Parse(salary), con);
            if (id_res == 0)
            {
                resume(id_pers_inf, urlResume, workExpTime, aboutMe, int.Parse(salary), con);
            }
            id_res = resume_poisk(urlResume, workExpTime, aboutMe, int.Parse(salary), con);
            resjob(id_res, id_job, con);
            resemp(id_res, id_emp, con);
            ressch(id_res, id_sch, con);
            Console.WriteLine();

        }
        var workExperience = docResume.DocumentNode.SelectSingleNode("//div[@data-qa='resume-block-experience']"); //блок с работами
        if (workExperience == null)
        {
            /*Console.WriteLine("Не указана информация об опыте работы");
            Console.WriteLine();*/
        }
        else
        {
            var personalElemHTML = workExperience.SelectNodes(".//span[@class='resume-block__title-text resume-block__title-text_sub']/span");
            if (personalElemHTML != null)
            {
                foreach (var info in personalElemHTML)
                {
                    workExpTime += info.InnerText + " ";
                }
                workExpTime.Trim();
                Console.WriteLine("Опыт работы: " + workExpTime);
            }

            personalElemHTML = workExperience.SelectNodes(".//div[@class='resume-block-item-gap']/div[@class='bloko-columns-row']/div[@class='resume-block-item-gap']");
            if (personalElemHTML != null)
            {

                foreach (var job in personalElemHTML)
                {
                    string when = job.SelectSingleNode(".//div[@class='bloko-column bloko-column_xs-4 bloko-column_s-2 bloko-column_m-2 bloko-column_l-2']").GetDirectInnerText();
                    string where = job.SelectSingleNode(".//div[@class='bloko-text bloko-text_strong']").InnerText;
                    string who = job.SelectSingleNode(".//div[@data-qa='resume-block-experience-position'][@class='bloko-text bloko-text_strong']").GetDirectInnerText();

                    jobs.Add(new Job(when, where, who));
                    id_lw = last_work_poisk(when, where, who, con);
                    if (id_lw == 0)
                    {
                        last_work(id_res, when, where, who, con);
                        id_lw = last_work_poisk(when, where, who, con);
                    }




                }

                Console.WriteLine("Работы:");
                foreach (Job job in jobs)
                {
                    Console.WriteLine("    " + job.GetInfo());

                }
            }
            Console.WriteLine();
        }

        var languagesTable = docResume.DocumentNode.SelectSingleNode("//div[@data-qa='resume-block-languages']/div[@class='resume-block-item-gap']"); //блок с знанием языков
        if (languagesTable == null)
        {
            /*Console.WriteLine("Нет указано знание языков");
            Console.WriteLine();*/
        }
        else
        {
            var languagesHTML = languagesTable.SelectNodes(".//div[@class='bloko-tag bloko-tag_inline']");
            Console.WriteLine("Знание языков: ");
            foreach (var lang in languagesHTML)
            {
                languages.Add(lang.InnerText);
            }
            languages.ForEach(i => Console.WriteLine("    {0}", i));

            Console.WriteLine();

        }

        var qualification = docResume.DocumentNode.SelectSingleNode("//div[@data-qa='resume-block-additional-education']/div[@class='resume-block-item-gap']"); //блок с повышением квалификации
        if (qualification == null)
        {
            /*Console.WriteLine("Не указана информация об повышении квалификации, курсах");
            Console.WriteLine();*/
        }
        else
        {
            var personalElemHTML = qualification.SelectNodes(".//div[@class='resume-block-item-gap']");
            if (personalElemHTML != null)
            {
                foreach (var edu in personalElemHTML)
                {
                    string when = edu.SelectSingleNode(".//div[@class='bloko-column bloko-column_xs-4 bloko-column_s-2 bloko-column_m-2 bloko-column_l-2']").GetDirectInnerText();
                    string where = edu.SelectSingleNode(".//div[@data-qa='resume-block-education-name']").InnerText;
                    string what = edu.SelectSingleNode(".//div[@data-qa='resume-block-education-organization']").GetDirectInnerText();
                    qualifications.Add(new Qualification(when, where, what));
                }

                Console.WriteLine("Повышение квалификации, курсы:");
                foreach (Qualification qual in qualifications)
                {
                    Console.WriteLine("    " + qual.GetInfo());
                    string d = qual.GetInfo();
                    String[] stroka = d.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    id_edu = education_poisk(stroka[0], "курсы", stroka[2], stroka[1], con);
                    if (id_edu == 0)
                    {
                        educationn(stroka[0], "курсы", stroka[2], stroka[1], con); //внесение информации об образование
                        id_edu = education_poisk(stroka[0], "курсы", stroka[2], stroka[1], con);
                    }
                }

                Console.WriteLine();

            }

        }

        var education = docResume.DocumentNode.SelectSingleNode("//div[@data-qa='resume-block-education']"); //блок с образованием
        if (education == null)
        {
            /*Console.WriteLine("Не указана информация об образовании");
            Console.WriteLine();*/
        }
        else
        {
            var personalElemHTML = education.SelectNodes(".//div[@class='resume-block-item-gap']/div[@class='bloko-columns-row']/div[@class='resume-block-item-gap']");
            if (personalElemHTML != null)
            {

                foreach (var edu in personalElemHTML)
                {
                    string when = edu.SelectSingleNode(".//div[@class='bloko-column bloko-column_xs-4 bloko-column_s-2 bloko-column_m-2 bloko-column_l-2']").GetDirectInnerText();
                    string where = edu.SelectSingleNode(".//div[@data-qa='resume-block-education-name']").InnerText;
                    string who = "";
                    var whoHTML = edu.SelectSingleNode(".//div[@data-qa='resume-block-education-organization']");
                    if (whoHTML != null)
                    {
                        who = whoHTML.InnerText;
                    }
                    educations.Add(new Education(when, where, who));
                }

                Console.WriteLine("Образование:");
                foreach (Education edu in educations)
                {
                    Console.WriteLine("    " + edu.GetInfo());
                    string d = edu.GetInfo();
                    String[] stroka = d.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    id_edu = education_poisk(stroka[0], " ", stroka[1], stroka[2], con);//внесение информации об образование
                    if (id_edu == 0)
                    {
                        educationn(stroka[0], " ", stroka[1], stroka[2], con); //внесение информации об образование
                        id_edu = education_poisk(stroka[0], " ", stroka[1], stroka[2], con);//внесение информации об образование
                    }
                    infedu(id_pers_inf, id_edu, con);
                }
                Console.WriteLine();
            }
        }
    }
    static void ParseSearched()
    {

        Console.Write("\nВведите слова для поиска: ");
        string searchParam = Console.ReadLine();
        Console.WriteLine();


        HtmlWeb web = new HtmlWeb();
        HtmlDocument doc = web.Load("https://perm.hh.ru/search/resume?relocation=living_or_relocation&gender=unknown&text=" +
            searchParam +
            "&ored_clusters=true&order_by=relevance&items_on_page=100&search_period=0&logic=normal&pos=full_text&exp_period=all_time");

        Console.WriteLine("=====================================Поиск вакансий....\n");
        var nodes = doc.DocumentNode.SelectNodes("//a[@class='serp-item__title']");
        //Console.WriteLine(nodes.Count); //кол-во резюме на странице поиска
        int i = 0;
        if (nodes != null)
        {
            foreach (var node in nodes)
            {
                string urlResume = "https://perm.hh.ru" + node.GetAttributeValue("href", null);
                Console.WriteLine(node.InnerText + " " + urlResume);
                Console.WriteLine();

                ParseResume(urlResume);
                i++;
                if (i == 5)
                    break;

            }
        }
    }
    static int last_work_poisk(string date_duration, string company, string job_title, NpgsqlConnection con)
    {
        string sql = $@"SELECT * FROM last_work WHERE date_duration = '{date_duration}' AND company = '{company}'AND job_title = '{job_title}' ";
        using var cmd = new NpgsqlCommand(sql, con);
        using NpgsqlDataReader rdr = cmd.ExecuteReader();
        if (rdr.Read())
        {
            return rdr.GetInt32(0);
        }
        else
        {
            return 0;
        }
    }
    static void personal_information(string surname, string first_name, string middle_name, int age, string sex, string mail, string phone_number, string birthday, string Citizenship, string loc, NpgsqlConnection con) // занисение информации в таблицу персональных данных
    {


        var sql = "INSERT INTO personal_information(surname,first_name,middle_name, age, sex, mail, phone_number,birthday, Citizenship,loc) VALUES(@surname,@first_name, @middle_name, @age, @sex, @mail, @phone_number, @birthday, @Citizenship,@loc)";
        var cmd = new NpgsqlCommand(sql, con);
        cmd.Parameters.AddWithValue("surname", surname);
        cmd.Parameters.AddWithValue("first_name", first_name);
        cmd.Parameters.AddWithValue("middle_name", middle_name);
        cmd.Parameters.AddWithValue("age", age);
        cmd.Parameters.AddWithValue("sex", sex);
        cmd.Parameters.AddWithValue("mail", mail);
        cmd.Parameters.AddWithValue("phone_number", phone_number);
        cmd.Parameters.AddWithValue("birthday", birthday);
        cmd.Parameters.AddWithValue("Citizenship", Citizenship);
        cmd.Parameters.AddWithValue("loc", loc);
        cmd.Prepare();
        cmd.ExecuteNonQuery();

    }
    static void educationn(string year, string education, string university, string specialty, NpgsqlConnection con) //внесение информации об образование
    {
        var sql = "INSERT INTO education(year_graduation,education, university, specialty) VALUES(@year_graduation, @education, @university, @specialty)";
        var cmd = new NpgsqlCommand(sql, con);
        cmd.Parameters.AddWithValue("year_graduation", year);
        cmd.Parameters.AddWithValue("education", education);
        cmd.Parameters.AddWithValue("university", university);
        cmd.Parameters.AddWithValue("specialty", specialty);
        cmd.Prepare();
        cmd.ExecuteNonQuery();
    }
    static void skilll(string skills, NpgsqlConnection con)//занисение информации о навыках и языках которые есть у человека
    {
        var sql = "INSERT INTO skills(skills) VALUES(@skills)";
        var cmd = new NpgsqlCommand(sql, con);
        cmd.Parameters.AddWithValue("skills", skills);
        cmd.Prepare();
        cmd.ExecuteNonQuery();

    }
    static void schedule(string schedule, NpgsqlConnection con)//заносит информацию о желаемом графике работы 

    {
        var sql = "INSERT INTO schedule(schedule) VALUES(@schedule)";
        var cmd = new NpgsqlCommand(sql, con);
        cmd.Parameters.AddWithValue("schedule", schedule);
        cmd.Prepare();
        cmd.ExecuteNonQuery();

    }
    static void resume(int id_pers_inf, string link, string experience, string about, int salary, NpgsqlConnection con) //информация о резюме
    {

        var sql = "INSERT INTO resume(id_pers_inf, link, experience,about, salary) VALUES(@id_pers_inf, @link, @experience,@about, @salary)";
        var cmd = new NpgsqlCommand(sql, con);
        cmd.Parameters.AddWithValue("id_pers_inf", id_pers_inf);
        cmd.Parameters.AddWithValue("link", link);
        cmd.Parameters.AddWithValue("experience", experience);
        cmd.Parameters.AddWithValue("about", about);
        cmd.Parameters.AddWithValue("salary", salary);

        cmd.Prepare();
        cmd.ExecuteNonQuery();

    }
    static void Employment(string employment, NpgsqlConnection con)//заносит информацию о желаемой занятости 
    {
        var sql = "INSERT INTO employment(employment) VALUES(@employment)";
        var cmd = new NpgsqlCommand(sql, con);
        cmd.Parameters.AddWithValue("employment", employment);
        cmd.Prepare();
        cmd.ExecuteNonQuery();

    }
    static void job_title(string job_title, NpgsqlConnection con)//заносит информацию о желаемоей должности
    {
        var sql = "INSERT INTO jobtitle(job_title) VALUES(@job_title)";
        var cmd = new NpgsqlCommand(sql, con);
        cmd.Parameters.AddWithValue("job_title", job_title);
        cmd.Prepare();
        cmd.ExecuteNonQuery();

    }
    static void last_work(int id_res, string date_duration, string company, string job_title, NpgsqlConnection con) //заносит информацию о прошлых местах работы
    {

        var sql = "INSERT INTO last_work(id_res,date_duration, company, job_title) VALUES(@id_res,@date_duration, @company, @job_title)";
        var cmd = new NpgsqlCommand(sql, con);
        cmd.Parameters.AddWithValue("id_res", id_res);
        cmd.Parameters.AddWithValue("date_duration", date_duration);
        cmd.Parameters.AddWithValue("company", company);
        cmd.Parameters.AddWithValue("job_title", job_title);

        cmd.Prepare();
        cmd.ExecuteNonQuery();

    }
    static void infedu(int id_pers_inf, int id_edu, NpgsqlConnection con)
    {


        var sql = "INSERT INTO infedu(id_pers_inf, id_edu) VALUES(@id_pers_inf, @id_edu)";
        var cmd = new NpgsqlCommand(sql, con);
        cmd.Parameters.AddWithValue("id_pers_inf", id_pers_inf);
        cmd.Parameters.AddWithValue("id_edu", id_edu);


        cmd.Prepare();
        cmd.ExecuteNonQuery();

    }
    static int personal_information_poisk(string surname, string first_name, string middle_name, int age, string sex, string mail, string phone_number, string birthday, string Citizenship, string loc, NpgsqlConnection con)
    {
        string sql = $@"SELECT * FROM personal_information WHERE surname = '{surname}' AND first_name = '{first_name}'AND middle_name = '{middle_name}' AND age = '{age}' AND sex = '{sex}' AND mail = '{mail}' AND phone_number = '{phone_number}' AND birthday ='{birthday}' AND citizenship = '{Citizenship}' AND loc = '{loc}' ";
        using var cmd = new NpgsqlCommand(sql, con);
        using NpgsqlDataReader rdr = cmd.ExecuteReader();
        if (rdr.Read())
        {
            return rdr.GetInt32(0);
        }
        else
        {
            return 0;
        }
    }

    static int schedule_poisk(string schedule, NpgsqlConnection con)
    {
        string sql = $@"SELECT * FROM schedule WHERE schedule = '{schedule}' ";
        using var cmd = new NpgsqlCommand(sql, con);
        using NpgsqlDataReader rdr = cmd.ExecuteReader();
        if (rdr.Read())
        {
            return rdr.GetInt32(0);
        }
        else
        {
            return 0;
        }
    }
    static int resume_poisk(string link, string experience, string about, int salary, NpgsqlConnection con)
    {
        string sql = $@"SELECT * FROM resume WHERE link = '{link}' AND experience = '{experience}'AND about = '{about}' AND salary = '{salary}' ";
        using var cmd = new NpgsqlCommand(sql, con);
        using NpgsqlDataReader rdr = cmd.ExecuteReader();
        if (rdr.Read())
        {
            return rdr.GetInt32(0);
        }
        else
        {
            return 0;
        }
    }
    static int jobtitle_poisk(string job_title, NpgsqlConnection con)
    {
        string sql = $@"SELECT * FROM jobtitle WHERE job_title = '{job_title}' ";
        using var cmd = new NpgsqlCommand(sql, con);
        using NpgsqlDataReader rdr = cmd.ExecuteReader();
        if (rdr.Read())
        {
            return rdr.GetInt32(0);
        }
        else
        {
            return 0;
        }
    }
    static int employment_poisk(string Employment, NpgsqlConnection con)
    {
        string sql = $@"SELECT * FROM employment WHERE employment = '{Employment}' ";
        using var cmd = new NpgsqlCommand(sql, con);
        using NpgsqlDataReader rdr = cmd.ExecuteReader();
        if (rdr.Read())
        {
            return rdr.GetInt32(0);
        }
        else
        {
            return 0;
        }
    }
    static int education_poisk(string year, string education, string university, string specialty, NpgsqlConnection con)
    {
        string sql = $@"SELECT * FROM education WHERE year_graduation = '{year}' AND education = '{education}'AND university = '{university}' AND specialty = '{specialty}'";
        using var cmd = new NpgsqlCommand(sql, con);
        using NpgsqlDataReader rdr = cmd.ExecuteReader();
        if (rdr.Read())
        {
            return rdr.GetInt32(0);
        }
        else
        {
            return 0;
        }
    }
    static int skills_poisk(string skill, NpgsqlConnection con)
    {
        string sql = $@"SELECT * FROM skills WHERE skills = '{skill}' ";
        using var cmd = new NpgsqlCommand(sql, con);

        using NpgsqlDataReader rdr = cmd.ExecuteReader();

        if (rdr.Read())
        {
            return rdr.GetInt32(0);
        }
        else
        {
            return 0;
        }
    }
    static void DocToHTML(string docName)
    {

        Console.WriteLine("\n=====================================Загрузка в html....\n");
        Document doc = new Document("../../../" + docName + ".doc");

        // Enable roundtrip information
        HtmlSaveOptions options = new HtmlSaveOptions();
        options.ExportRoundtripInformation = true;

        // Save as HTML
        doc.Save("../../../" + docName + ".html", options);
    }
    static void infskill(int id_pers_inf, int id_skill, NpgsqlConnection con)
    {


        var sql = "INSERT INTO infskill(id_pers_inf, id_skill) VALUES(@id_pers_inf, @id_skill)";
        var cmd = new NpgsqlCommand(sql, con);
        cmd.Parameters.AddWithValue("id_pers_inf", id_pers_inf);
        cmd.Parameters.AddWithValue("id_skill", id_skill);


        cmd.Prepare();
        cmd.ExecuteNonQuery();

    }
    static void resjob(int id_res, int id_job, NpgsqlConnection con)
    {


        var sql = "INSERT INTO resjob(id_res, id_job) VALUES(@id_res, @id_job)";
        var cmd = new NpgsqlCommand(sql, con);
        cmd.Parameters.AddWithValue("id_res", id_res);
        cmd.Parameters.AddWithValue("id_job", id_job);


        cmd.Prepare();
        cmd.ExecuteNonQuery();

    }
    static void resemp(int id_res, int id_emp, NpgsqlConnection con)
    {


        var sql = "INSERT INTO resemp(id_res, id_emp) VALUES(@id_res, @id_emp)";
        var cmd = new NpgsqlCommand(sql, con);
        cmd.Parameters.AddWithValue("id_res", id_res);
        cmd.Parameters.AddWithValue("id_emp", id_emp);


        cmd.Prepare();
        cmd.ExecuteNonQuery();

    }
    static void ressch(int id_res, int id_sch, NpgsqlConnection con)
    {


        var sql = "INSERT INTO ressch(id_res, id_sch) VALUES(@id_res, @id_sch)";
        var cmd = new NpgsqlCommand(sql, con);
        cmd.Parameters.AddWithValue("id_res", id_res);
        cmd.Parameters.AddWithValue("id_sch", id_sch);


        cmd.Prepare();
        cmd.ExecuteNonQuery();

    }
    public static void Main()
    {
        //нужно добавить удаление html страницы, если мы конвертировали из дока.
        //если поменять док файл руками, то он после конвертации не будет парситься

        ParseResume("https://perm.hh.ru/resume/96fe30a1ff0b77ed4e0039ed1f49353247526a");// страница резюме влада
        ParseSearched(); // просматривает все резюме на первой странице поиска. пока что ограничил до 5 резюме.                                                            //ParseSearched(); // просматривает все резюме на первой странице поиска. пока что ограничил до 5 резюме.

        //Тут будет вызов парсинга с сайта и с файлов

    }
}

