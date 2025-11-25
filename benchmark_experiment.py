"""
Вычислительный эксперимент для сравнения производительности двух версий приложения голосования.
Сравнивает Flask (версия 1) и Django (версия 2) по различным метрикам.
"""

import time
import sqlite3
import statistics
import json
from datetime import datetime

class BenchmarkExperiment:
    def __init__(self):
        self.results = {
            'flask': {},
            'django': {},
            'comparison': {}
        }
    
    def measure_database_operations(self, db_path, framework_name):
        """Измеряет производительность операций с базой данных"""
        results = {
            'insert': [],
            'select': [],
            'update': [],
            'delete': []
        }
        
        conn = sqlite3.connect(db_path)
        cursor = conn.cursor()
        
        # Создание тестовой таблицы
        cursor.execute('''
            CREATE TABLE IF NOT EXISTS benchmark_test (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                name TEXT,
                value INTEGER,
                created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
            )
        ''')
        conn.commit()
        
        # Тест INSERT
        for i in range(100):
            start = time.time()
            cursor.execute('INSERT INTO benchmark_test (name, value) VALUES (?, ?)', 
                         (f'test_{i}', i))
            conn.commit()
            results['insert'].append(time.time() - start)
        
        # Тест SELECT
        for i in range(100):
            start = time.time()
            cursor.execute('SELECT * FROM benchmark_test WHERE value = ?', (i,))
            cursor.fetchone()
            results['select'].append(time.time() - start)
        
        # Тест UPDATE
        for i in range(100):
            start = time.time()
            cursor.execute('UPDATE benchmark_test SET value = ? WHERE id = ?', 
                         (i * 2, i + 1))
            conn.commit()
            results['update'].append(time.time() - start)
        
        # Тест DELETE
        for i in range(100):
            start = time.time()
            cursor.execute('DELETE FROM benchmark_test WHERE id = ?', (i + 1,))
            conn.commit()
            results['delete'].append(time.time() - start)
        
        conn.close()
        
        # Вычисление средних значений
        avg_results = {}
        for operation, times in results.items():
            avg_results[operation] = {
                'mean': statistics.mean(times) * 1000,  # в миллисекундах
                'median': statistics.median(times) * 1000,
                'stdev': statistics.stdev(times) * 1000 if len(times) > 1 else 0,
                'min': min(times) * 1000,
                'max': max(times) * 1000
            }
        
        return avg_results
    
    def measure_query_complexity(self, db_path, framework_name):
        """Измеряет производительность сложных запросов"""
        results = {
            'join': [],
            'aggregate': [],
            'subquery': []
        }
        
        conn = sqlite3.connect(db_path)
        cursor = conn.cursor()
        
        # Тест JOIN
        for i in range(50):
            start = time.time()
            cursor.execute('''
                SELECT u.id, u.username, COUNT(v.id) as vote_count
                FROM user u
                LEFT JOIN vote v ON u.id = v.user_id
                GROUP BY u.id, u.username
            ''')
            cursor.fetchall()
            results['join'].append(time.time() - start)
        
        # Тест AGGREGATE
        for i in range(50):
            start = time.time()
            cursor.execute('''
                SELECT poll_id, COUNT(*) as total_votes, SUM(vote_count) as sum_votes
                FROM poll_option
                GROUP BY poll_id
            ''')
            cursor.fetchall()
            results['aggregate'].append(time.time() - start)
        
        # Тест SUBQUERY
        for i in range(50):
            start = time.time()
            cursor.execute('''
                SELECT * FROM poll
                WHERE id IN (SELECT poll_id FROM vote GROUP BY poll_id HAVING COUNT(*) > 0)
            ''')
            cursor.fetchall()
            results['subquery'].append(time.time() - start)
        
        conn.close()
        
        avg_results = {}
        for operation, times in results.items():
            if times:
                avg_results[operation] = {
                    'mean': statistics.mean(times) * 1000,
                    'median': statistics.median(times) * 1000,
                    'stdev': statistics.stdev(times) * 1000 if len(times) > 1 else 0
                }
        
        return avg_results
    
    def run_experiment(self):
        """Запускает полный эксперимент"""
        print("=" * 60)
        print("Вычислительный эксперимент: Сравнение Flask vs Django")
        print("=" * 60)
        print()
        
        # Тест операций с БД
        print("1. Тестирование операций с базой данных...")
        print("-" * 60)
        
        flask_db = 'version1_flask/voting.db'
        django_db = 'version2_django/db.sqlite3'
        
        try:
            self.results['flask']['db_operations'] = self.measure_database_operations(flask_db, 'Flask')
            print("✓ Flask: операции с БД измерены")
        except Exception as e:
            print(f"✗ Flask: ошибка - {e}")
            self.results['flask']['db_operations'] = None
        
        try:
            self.results['django']['db_operations'] = self.measure_database_operations(django_db, 'Django')
            print("✓ Django: операции с БД измерены")
        except Exception as e:
            print(f"✗ Django: ошибка - {e}")
            self.results['django']['db_operations'] = None
        
        print()
        
        # Тест сложных запросов
        print("2. Тестирование сложных SQL запросов...")
        print("-" * 60)
        
        try:
            self.results['flask']['complex_queries'] = self.measure_query_complexity(flask_db, 'Flask')
            print("✓ Flask: сложные запросы измерены")
        except Exception as e:
            print(f"✗ Flask: ошибка - {e}")
            self.results['flask']['complex_queries'] = None
        
        try:
            self.results['django']['complex_queries'] = self.measure_query_complexity(django_db, 'Django')
            print("✓ Django: сложные запросы измерены")
        except Exception as e:
            print(f"✗ Django: ошибка - {e}")
            self.results['django']['complex_queries'] = None
        
        print()
        
        # Сравнение результатов
        self.compare_results()
        
        # Вывод результатов
        self.print_results()
        
        # Сохранение в файл
        self.save_results()
    
    def compare_results(self):
        """Сравнивает результаты двух версий"""
        if (self.results['flask'].get('db_operations') and 
            self.results['django'].get('db_operations')):
            
            comparison = {}
            for operation in ['insert', 'select', 'update', 'delete']:
                flask_mean = self.results['flask']['db_operations'][operation]['mean']
                django_mean = self.results['django']['db_operations'][operation]['mean']
                
                if django_mean > 0:
                    ratio = flask_mean / django_mean
                    faster = 'Flask' if ratio < 1 else 'Django'
                    comparison[operation] = {
                        'flask_ms': flask_mean,
                        'django_ms': django_mean,
                        'ratio': ratio,
                        'faster': faster,
                        'difference_percent': abs((1 - ratio) * 100)
                    }
            
            self.results['comparison']['db_operations'] = comparison
    
    def print_results(self):
        """Выводит результаты эксперимента"""
        print("=" * 60)
        print("РЕЗУЛЬТАТЫ ЭКСПЕРИМЕНТА")
        print("=" * 60)
        print()
        
        if self.results['comparison'].get('db_operations'):
            print("Сравнение операций с БД (среднее время в мс):")
            print("-" * 60)
            print(f"{'Операция':<15} {'Flask (мс)':<15} {'Django (мс)':<15} {'Быстрее':<15} {'Разница %':<15}")
            print("-" * 60)
            
            for op, data in self.results['comparison']['db_operations'].items():
                print(f"{op:<15} {data['flask_ms']:<15.3f} {data['django_ms']:<15.3f} "
                      f"{data['faster']:<15} {data['difference_percent']:<15.2f}")
            print()
        
        # Детальные результаты Flask
        if self.results['flask'].get('db_operations'):
            print("Детальные результаты Flask:")
            print("-" * 60)
            for op, data in self.results['flask']['db_operations'].items():
                print(f"  {op}: среднее={data['mean']:.3f}мс, "
                      f"медиана={data['median']:.3f}мс, "
                      f"мин={data['min']:.3f}мс, макс={data['max']:.3f}мс")
            print()
        
        # Детальные результаты Django
        if self.results['django'].get('db_operations'):
            print("Детальные результаты Django:")
            print("-" * 60)
            for op, data in self.results['django']['db_operations'].items():
                print(f"  {op}: среднее={data['mean']:.3f}мс, "
                      f"медиана={data['median']:.3f}мс, "
                      f"мин={data['min']:.3f}мс, макс={data['max']:.3f}мс")
            print()
    
    def save_results(self):
        """Сохраняет результаты в JSON файл"""
        filename = f'benchmark_results_{datetime.now().strftime("%Y%m%d_%H%M%S")}.json'
        with open(filename, 'w', encoding='utf-8') as f:
            json.dump(self.results, f, indent=2, ensure_ascii=False)
        print(f"Результаты сохранены в файл: {filename}")

if __name__ == '__main__':
    experiment = BenchmarkExperiment()
    experiment.run_experiment()

