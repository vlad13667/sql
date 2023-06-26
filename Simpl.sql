--Таблица личная информация
create table Personal_Information(
    Id_Personal_Information bigserial primary key,--Id личной информации для связи таблиц
    Full_Name  text not null,--Полное имя
    Age int not null--Возраст
        check ( age > 0 ),
    Sex boolean not null,--Пол
    Mail text,--Почта
    Phone_Number text,--Телефон
    Loc text--Место жительства
);
--Таблица навыков
create table Skills(
    Id_Skills bigserial primary key ,--Id навыков для связи таблиц
    Skills text--Навык
);
--Таблица связи личной информации и навыков
create table InfSkill
(
    Id_InfSkill bigserial primary key ,--Id личной информации и навыков
    Id_Pers_Inf bigserial not null constraint Personal_Information_ID_fk references Personal_Information on update cascade on delete cascade,--Id связь с личной информацией
    Id_Skill bigserial not null constraint Skills_ID_fk references Skills on update cascade on delete cascade--Id связь с навыками
);
--Таблица образования
create table Education(
  Id_education  bigserial primary key ,--Id образования для связи таблиц
  Education text not null,--Поля для уровня образования нет/курсы/высшее и тд
  University text,--Университет/учебное заведение
  Specialty text--Направление
);
--Таблица связи личной информации и образования
create table InfEdu(
    Id_InfEdu bigserial primary key ,--Id личной информации и образования
    Id_Pers_Inf bigserial not null constraint Personal_Information_ID_fk references Personal_Information on update cascade on delete cascade,--Id связь с личной информацией
    Id_Edu bigserial not null constraint Education_ID_fk references Education on update cascade on delete cascade--Id связь с образование
);
--Таблица резюме
create table Resume(
    Id_Resume bigserial primary key ,--Id резюме для связи таблиц
    Id_Pers_Inf bigserial not null constraint Personal_Information_ID_fk references Personal_Information on update cascade on delete cascade,--Id связь с личной информацией
    Link text not null,--Ссылка на резюме
    Experience integer not null,--Опыт работы
    Salary integer not null--Желаемая ЗП
        check ( Salary > 0 )
);
--Таблица занятости
create table Employment(
    Id_Employment bigserial primary key ,--Id занятости для связи таблиц
    Employment text not null --Занятость
);
--Таблица связи резюме и занятости
create table ResEmp(
    Id_ResEmp bigserial primary key ,--Id резюме и занятости
    Id_Res bigserial not null constraint Resume_ID_fk references Resume on update cascade on delete cascade,--Id связь с резюме
    Id_Emp bigserial not null constraint Employment_ID_fk references Employment on update cascade on delete cascade--Id связь с занятостью
);
--Таблица желаемой должности
create table JobTitle(
    Id_Job_Title bigserial primary key ,--Id должности для связи таблиц
    Job_Title text not null --должность
);
--Таблица связи резюме и должности
create table ResJob(
   Id_ResJob bigserial primary key ,--Id резюме и должности
   Id_Res bigserial not null constraint Resume_ID_fk references Resume on update cascade on delete cascade,--Id связь с резюме
   Id_Job bigserial not null constraint JobTitle_ID_fk references JobTitle on update cascade on delete cascade--Id связь с должность
);
--Таблица job сайтов
create table Job_Site(
    Id_Job_Site bigserial primary key ,--Id должности для связи таблиц
    Id_Res bigserial not null constraint Resume_ID_fk references Resume on update cascade on delete cascade,--Id связь с резюме
    Job_Site_Link text not null--Ссылка на job сайт
);

--Очистка
drop table Job_Site;
drop table ResJob;
drop table JobTitle;
drop table ResEmp;
drop table Employment;
drop table InfEdu;
drop table Education;
drop table InfSkill;
drop table Skills;
drop table Resume;
drop table Personal_Information;
