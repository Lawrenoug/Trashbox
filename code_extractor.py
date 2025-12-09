#!/usr/bin/env python3
"""
Godot 项目代码提取器 - 提取所有 .gd 和 .cs 文件内容
专门为 Gemini 3 Pro 设计
"""

import os
import re
from pathlib import Path
from datetime import datetime
import textwrap


class GodotCodeExtractor:
    def __init__(self, project_root=".", max_file_size=50000):  # 50KB 限制
        self.root = Path(project_root).absolute()
        self.output_lines = []
        self.max_file_size = max_file_size

        # 统计信息
        self.stats = {
            'gd_files': [],
            'cs_files': [],
            'tscn_files': [],
            'other_files': [],
            'total_lines': 0,
            'total_size': 0
        }

    def extract_project_code(self):
        """提取项目中的所有代码"""
        print(f"📂 正在提取项目代码: {self.root}")

        # 生成项目概览
        self._create_project_overview()

        # 提取 GDScript 文件
        self._extract_gdscripts()

        # 提取 C# 文件
        self._extract_csharp_files()

        # 提取场景文件关键信息
        self._extract_scenes_info()

        # 提取项目配置
        self._extract_project_config()

        # 保存结果
        return self._save_extracted_code()

    def _create_project_overview(self):
        """创建项目概览"""
        self.output_lines.append("# 🎮 GODOT 项目完整代码库")
        self.output_lines.append(f"生成时间: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}")
        self.output_lines.append(f"项目路径: {self.root}")
        self.output_lines.append("\n---\n")

    def _extract_gdscripts(self):
        """提取所有 GDScript 文件内容"""
        gd_files = list(self.root.rglob("*.gd"))

        if not gd_files:
            self.output_lines.append("## 📜 GDScript 文件\n")
            self.output_lines.append("未找到 .gd 文件")
            return

        self.output_lines.append("## 📜 GDScript 文件\n")

        for gd_file in sorted(gd_files):
            try:
                file_size = os.path.getsize(gd_file)
                if file_size > self.max_file_size:
                    print(f"⚠️  跳过大文件: {gd_file} ({file_size / 1024:.1f}KB)")
                    continue

                rel_path = gd_file.relative_to(self.root)
                self.stats['gd_files'].append(str(rel_path))

                # 读取文件内容
                with open(gd_file, 'r', encoding='utf-8', errors='ignore') as f:
                    content = f.read()

                # 计算行数
                lines = content.split('\n')
                line_count = len(lines)

                # 添加到输出
                self.output_lines.append(f"### 📄 {rel_path}")
                self.output_lines.append(f"行数: {line_count} | 大小: {file_size:,} 字节\n")

                # 添加完整代码
                self.output_lines.append("```gdscript")
                self.output_lines.append(content)
                self.output_lines.append("```\n")

                # 更新统计
                self.stats['total_lines'] += line_count
                self.stats['total_size'] += file_size

            except Exception as e:
                self.output_lines.append(f"### ⚠️ {rel_path}")
                self.output_lines.append(f"读取失败: {e}\n")

    def _extract_csharp_files(self):
        """提取所有 C# 文件内容"""
        cs_files = list(self.root.rglob("*.cs"))

        if not cs_files:
            self.output_lines.append("## 📟 C# 文件\n")
            self.output_lines.append("未找到 .cs 文件")
            return

        self.output_lines.append("## 📟 C# 文件\n")

        for cs_file in sorted(cs_files):
            try:
                file_size = os.path.getsize(cs_file)
                if file_size > self.max_file_size:
                    print(f"⚠️  跳过大文件: {cs_file} ({file_size / 1024:.1f}KB)")
                    continue

                rel_path = cs_file.relative_to(self.root)
                self.stats['cs_files'].append(str(rel_path))

                # 读取文件内容
                with open(cs_file, 'r', encoding='utf-8', errors='ignore') as f:
                    content = f.read()

                # 计算行数
                lines = content.split('\n')
                line_count = len(lines)

                # 添加到输出
                self.output_lines.append(f"### 📄 {rel_path}")
                self.output_lines.append(f"行数: {line_count} | 大小: {file_size:,} 字节\n")

                # 添加完整代码
                self.output_lines.append("```csharp")
                self.output_lines.append(content)
                self.output_lines.append("```\n")

                # 更新统计
                self.stats['total_lines'] += line_count
                self.stats['total_size'] += file_size

            except Exception as e:
                self.output_lines.append(f"### ⚠️ {rel_path}")
                self.output_lines.append(f"读取失败: {e}\n")

    def _extract_scenes_info(self):
        """提取场景文件的关键信息"""
        tscn_files = list(self.root.rglob("*.tscn"))

        if not tscn_files:
            return

        self.output_lines.append("## 🎬 场景文件 (.tscn)\n")

        for tscn_file in sorted(tscn_files):
            try:
                rel_path = tscn_file.relative_to(self.root)
                self.stats['tscn_files'].append(str(rel_path))

                # 读取文件内容
                with open(tscn_file, 'r', encoding='utf-8', errors='ignore') as f:
                    content = f.read()

                # 只提取关键信息（前50行）
                lines = content.split('\n')
                key_lines = []

                # 收集包含节点、脚本、资源引用的行
                for line in lines[:50]:
                    line = line.strip()
                    if not line:
                        continue

                    # 重要元素
                    important_keys = [
                        '[gd_scene', '[node', 'type=', 'name=', 'script=',
                        'instance=', 'path=', 'texture=', 'mesh='
                    ]

                    if any(key in line for key in important_keys):
                        key_lines.append(line[:200])  # 限制每行长度

                # 添加到输出
                self.output_lines.append(f"### 🎬 {rel_path}")
                self.output_lines.append(f"大小: {os.path.getsize(tscn_file):,} 字节\n")

                if key_lines:
                    self.output_lines.append("**关键信息:**")
                    self.output_lines.append("```")
                    self.output_lines.append('\n'.join(key_lines[:20]))
                    if len(key_lines) > 20:
                        self.output_lines.append("...")
                    self.output_lines.append("```\n")

            except Exception as e:
                self.output_lines.append(f"### ⚠️ {rel_path}")
                self.output_lines.append(f"读取失败: {e}\n")

    def _extract_project_config(self):
        """提取项目配置"""
        project_file = self.root / "project.godot"

        if not project_file.exists():
            return

        try:
            with open(project_file, 'r', encoding='utf-8') as f:
                content = f.read()

            self.output_lines.append("## ⚙️ 项目配置 (project.godot)\n")
            self.output_lines.append("```ini")
            self.output_lines.append(content[:2000])  # 只显示前2000字符
            if len(content) > 2000:
                self.output_lines.append("...")
            self.output_lines.append("```\n")

        except Exception as e:
            self.output_lines.append(f"## ⚙️ 项目配置\n")
            self.output_lines.append(f"读取失败: {e}\n")

    def _save_extracted_code(self):
        """保存提取的代码"""
        # 添加统计信息
        self.output_lines.append("\n---\n")
        self.output_lines.append("## 📊 统计信息\n")

        self.output_lines.append(f"- **总代码行数**: {self.stats['total_lines']:,}")
        self.output_lines.append(f"- **总文件大小**: {self.stats['total_size'] / 1024:.1f} KB")
        self.output_lines.append(f"- **GDScript 文件数**: {len(self.stats['gd_files'])}")
        if self.stats['gd_files']:
            self.output_lines.append("  - " + "\n  - ".join(self.stats['gd_files'][:10]))
            if len(self.stats['gd_files']) > 10:
                self.output_lines.append(f"  - ... 还有 {len(self.stats['gd_files']) - 10} 个文件")

        self.output_lines.append(f"- **C# 文件数**: {len(self.stats['cs_files'])}")
        if self.stats['cs_files']:
            self.output_lines.append("  - " + "\n  - ".join(self.stats['cs_files'][:10]))
            if len(self.stats['cs_files']) > 10:
                self.output_lines.append(f"  - ... 还有 {len(self.stats['cs_files']) - 10} 个文件")

        if self.stats['tscn_files']:
            self.output_lines.append(f"- **场景文件数**: {len(self.stats['tscn_files'])}")

        # 保存到文件
        output_file = self.root / "FULL_CODE_EXTRACT.md"
        content = '\n'.join(self.output_lines)

        with open(output_file, 'w', encoding='utf-8') as f:
            f.write(content)

        file_size_kb = len(content.encode('utf-8')) / 1024

        print(f"\n✅ 代码提取完成!")
        print(f"📄 输出文件: {output_file}")
        print(f"📏 文件大小: {file_size_kb:.1f} KB")
        print(f"📝 总行数: {len(self.output_lines):,}")
        print(f"🔤 字符数: {len(content):,}")
        print(f"📊 统计:")
        print(f"  - GDScript 文件: {len(self.stats['gd_files'])}")
        print(f"  - C# 文件: {len(self.stats['cs_files'])}")
        print(f"  - 总代码行: {self.stats['total_lines']:,}")

        return output_file


def main():
    """主函数"""
    print("🚀 Godot 项目代码提取器")
    print("=" * 60)
    print("📝 功能: 提取所有 .gd 和 .cs 文件的完整代码内容")
    print("🎯 用途: 上传给 AI (Gemini 3 Pro) 进行分析和协助\n")

    import sys

    # 确定项目路径
    if len(sys.argv) > 1:
        project_path = sys.argv[1]
    else:
        project_path = "."  # 当前目录

    # 检查路径
    root = Path(project_path).absolute()
    print(f"目标目录: {root}")

    if not root.exists():
        print(f"❌ 错误: 目录不存在 - {root}")
        return

    # 检查是否是 Godot 项目
    project_file = root / "project.godot"
    if not project_file.exists():
        print("⚠️  警告: 未找到 project.godot 文件")
        print("这可能不是 Godot 项目目录")
        response = input("是否继续? (y/n): ")
        if response.lower() != 'y':
            print("❌ 已取消")
            return

    # 设置文件大小限制
    max_size_input = input("设置最大文件大小限制 (KB, 默认 50): ").strip()
    if max_size_input:
        try:
            max_size_kb = int(max_size_input)
            max_size = max_size_kb * 1024
        except:
            max_size = 50 * 1024
            print(f"⚠️  输入无效，使用默认值: 50KB")
    else:
        max_size = 50 * 1024

    print(f"\n🔄 正在扫描项目...")

    # 运行代码提取
    try:
        extractor = GodotCodeExtractor(project_path, max_file_size=max_size)
        output_file = extractor.extract_project_code()

        print("\n🎯 上传到 Gemini 3 Pro:")
        print("=" * 60)
        print("1. 访问: https://gemini.google.com")
        print("2. 点击「上传文件」按钮")
        print(f"3. 选择: {output_file}")
        print("4. 输入你的问题，例如:")
        print("   - '请帮我分析这个项目的代码结构'")
        print("   - '这个玩家控制器脚本有问题，帮我修复'")
        print("   - '解释这个 C# 脚本的功能'")

        # 检查文件是否过大
        file_size_mb = os.path.getsize(output_file) / (1024 * 1024)
        if file_size_mb > 10:
            print(f"\n⚠️  警告: 输出文件较大 ({file_size_mb:.1f} MB)")
            print("建议上传时选择 '文档分析' 模式")

        # 询问是否查看部分内容
        if input("\n查看前几行内容? (y/n): ").lower() == 'y':
            with open(output_file, 'r', encoding='utf-8') as f:
                preview = ''.join([next(f) for _ in range(20)])
                print("\n预览内容:")
                print(preview)
                print("...")

    except KeyboardInterrupt:
        print("\n❌ 用户中断")
    except Exception as e:
        print(f"\n❌ 错误: {e}")
        import traceback
        traceback.print_exc()


if __name__ == "__main__":
    main()