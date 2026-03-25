import tkinter as tk
from tkinter import ttk, messagebox, filedialog
import json
import base64
import subprocess
from typing import Dict, List, Tuple, Optional, Any, Callable

WINDOW_TITLE = "自动化计算工具-王国强-202603"
WINDOW_SIZE = "533x550"
WINDOW_MAX_HEIGHT = 500
WINDOW_PADDING = 10
BUTTON_PADDING = 5
ENTRY_WIDTH = 30
RESULT_HEIGHT = 20
RESULT_WIDTH = 80

COLORS = {
    'background': '#f0f0f0',
    'field_background': '#f0f0f0',
    'title': 'blue',
    'separator': 'gray',
    'fault': 'darkred',
    'result': 'darkgreen',
    'error': '#e74c3c',
    'info': '#3498db',
    'success': '#27ae60',
    'text': '#2c3e50',
    'active_bg': '#e0e0e0',
    'select_bg': '#d0e8f0',
    'title_bg': '#e6f3ff',
    'tab_selected': '#4a90e2',
    'tab_selected_text': '#ffffff'
}

FONTS = {
    'default': ('微软雅黑', 9),
    'bold': ('微软雅黑', 10, 'bold'),
    'title': ('微软雅黑', 10, 'bold'),
    'result': ('微软雅黑', 11),
    'label': ('微软雅黑', 10),
    'small': ('微软雅黑', 8)
}

TAB_NAMES = {
    'data': '  打包数据查看  ',
    'status': '  状态字查看  ',
    'memory': '  内存映象网计算  '
}

CALC_TYPES = {
    1: "模拟量计算\n16进制地址",
    2: "模拟量计算\nR地址",
    3: "数字量计算\n16进制地址和位数",
    4: "数字量计算\nM地址"
}

def hex_to_int(hex_str: str) -> int:
    return int(hex_str, 16)

def int_to_hex(int_val: int) -> str:
    return f"{int_val:04X}"

def calculate_analog_end_mem_address(initial_mem_address: str, initial_r_address: int, end_r_address: int) -> str:
    initial_mem_address = hex_to_int(initial_mem_address)
    offset = (end_r_address - initial_r_address) * 2
    end_mem_address = initial_mem_address + offset
    return int_to_hex(end_mem_address)

def calculate_analog_end_r_address(initial_mem_address: str, initial_r_address: int, end_mem_address: str) -> int:
    initial_mem_address = hex_to_int(initial_mem_address)
    end_mem_address = hex_to_int(end_mem_address)
    offset = (end_mem_address - initial_mem_address) // 2
    return initial_r_address + offset

def calculate_digital_end_mem_address_and_bit(initial_mem_address: str, initial_m_address: int, end_m_address: int) -> Tuple[str, int]:
    initial_mem_address = hex_to_int(initial_mem_address)
    offset = (end_m_address - initial_m_address) // 8
    bit_position = (end_m_address - initial_m_address) % 8
    end_mem_address = initial_mem_address + offset
    return int_to_hex(end_mem_address), bit_position

def calculate_digital_end_m_address(initial_mem_address: str, initial_m_address: int, end_mem_address: str, bit_position: int) -> int:
    initial_mem_address = hex_to_int(initial_mem_address)
    end_mem_address = hex_to_hex(end_mem_address)
    offset = (end_mem_address - initial_mem_address) * 8
    return initial_m_address + offset + bit_position

def hex_to_hex(hex_str: str) -> int:
    return int(hex_str, 16)

class OPUChecker:
    def __init__(self, root: tk.Tk):
        self.root = root
        self.root.title(WINDOW_TITLE)
        self.root.geometry(WINDOW_SIZE)
        
        self._init_styles()
        self._init_variables()
        self.create_widgets()

    def _init_styles(self):
        self.style = ttk.Style()
        self.style.theme_use('clam')
        self.style.configure('TButton', font=FONTS['default'], padding=BUTTON_PADDING)
        self.style.configure('TLabel', font=FONTS['default'], padding=3)
        self.style.configure('TEntry', fieldbackground=COLORS['field_background'])
        self.style.configure('TNotebook', background=COLORS['background'])
        self.style.configure('TNotebook.Tab', font=FONTS['bold'], padding=[10, 5])

    def _init_variables(self):
        self.first_word_map: Dict[str, Any] = {}
        self.second_word_map: Dict[str, Any] = {}
        self.first_name = "第一个字"
        self.second_name = "第二个字"
        self.groups_data: Dict[str, Any] = {}
        self.current_group: Optional[str] = None
        self.memory_entries: Dict[str, tk.Entry] = {}
        self.memory_result_label: Optional[tk.Label] = None
        self.memory_calc_type = tk.IntVar(value=1)
        self.status_word_map: Dict[str, str] = {}
        self.status_groups_data: Dict[str, Any] = {}
        self.current_status_word: Optional[str] = None
        self.current_status_group: Optional[str] = None
        self.current_status_name = "状态字"

    def create_widgets(self):
        self._create_top_buttons()
        self._create_notebook()
        self._create_data_tab()
        self._create_status_tab()
        self._create_memory_tab()

    def _create_top_buttons(self):
        top_button_frame = ttk.Frame(self.root)
        top_button_frame.pack(fill=tk.X, padx=WINDOW_PADDING * 2, pady=WINDOW_PADDING)

        buttons = [
            ("导入", self.import_json, "import_btn"),
            ("列表", self.show_group_menu, "group_btn"),
            ("计算器", self.open_calculator, "calc_btn"),
            ("关于", self.show_about, "about_btn")
        ]

        for text, command, attr_name in buttons:
            btn = ttk.Button(top_button_frame, text=text, command=command)
            btn.pack(side=tk.LEFT, padx=BUTTON_PADDING)
            setattr(self, attr_name, btn)

    def _create_notebook(self):
        self.notebook = ttk.Notebook(self.root)
        self.notebook.pack(fill=tk.BOTH, expand=True, padx=WINDOW_PADDING, pady=5)

        self.data_tab = ttk.Frame(self.notebook)
        self.status_tab = ttk.Frame(self.notebook)
        self.memory_tab = ttk.Frame(self.notebook)

        self.notebook.add(self.data_tab, text=TAB_NAMES['data'])
        self.notebook.add(self.status_tab, text=TAB_NAMES['status'])
        self.notebook.add(self.memory_tab, text=TAB_NAMES['memory'])

        self._configure_tab_styles()
        self.notebook.bind('<<NotebookTabChanged>>', self._on_tab_changed)

    def _create_data_tab(self):
        self.current_group_label = ttk.Label(
            self.data_tab,
            text="当前选择的打包字：未选择",
            font=FONTS['default'],
            foreground=COLORS['title']
        )
        self.current_group_label.pack(pady=5, anchor='w', padx=WINDOW_PADDING * 2)

        input_frame = ttk.Frame(self.data_tab)
        input_frame.pack(fill=tk.X, padx=WINDOW_PADDING * 3, pady=5)

        self.first_entry, self.first_name_label = self._create_input_field(
            parent_frame=input_frame,
            label_text="第一个字(十进制):",
            name_text=self.first_name,
            entry_width=ENTRY_WIDTH,
            pady=8,
            command=lambda: self.check_fault(1)
        )

        self.second_entry, self.second_name_label = self._create_input_field(
            parent_frame=input_frame,
            label_text="第二个字(十进制):",
            name_text=self.second_name,
            entry_width=ENTRY_WIDTH,
            pady=5,
            command=lambda: self.check_fault(2)
        )

        self.result_text = self._create_result_text(self.data_tab)
        self._configure_result_tags(self.result_text)

    def _create_status_tab(self):
        self.current_status_label = ttk.Label(
            self.status_tab,
            text="当前选择的状态字：未选择",
            font=FONTS['default'],
            foreground=COLORS['title']
        )
        self.current_status_label.pack(pady=5, anchor='w', padx=WINDOW_PADDING * 2)

        self.status_result_text = self._create_result_text(self.status_tab)
        self._configure_result_tags(self.status_result_text, is_status=True)

    def _create_memory_tab(self):
        main_frame = ttk.Frame(self.memory_tab)
        main_frame.pack(fill=tk.BOTH, expand=True, padx=WINDOW_PADDING, pady=WINDOW_PADDING)

        left_frame = ttk.Frame(main_frame, width=150)
        left_frame.pack(side=tk.LEFT, fill=tk.Y, padx=(0, 10))
        left_frame.pack_propagate(False)

        right_frame = ttk.Frame(main_frame)
        right_frame.pack(side=tk.LEFT, fill=tk.BOTH, expand=True)

        self._create_calc_type_selector(left_frame)
        self.memory_input_frame = ttk.LabelFrame(right_frame, text="输入参数", padding=15)
        self.memory_input_frame.pack(fill=tk.X, pady=(0, 15))
        self.switch_memory_calc_type()

        button_frame = ttk.Frame(right_frame)
        button_frame.pack(fill=tk.X, pady=(0, 15))
        ttk.Button(button_frame, text="开始计算", command=self.on_memory_calculate, width=20).pack()

        result_frame = ttk.LabelFrame(right_frame, text="计算结果", padding=15)
        result_frame.pack(fill=tk.X)
        self.memory_result_label = tk.Label(
            result_frame,
            text="",
            font=FONTS['title'],
            foreground=COLORS['error'],
            bg=COLORS['background'],
            pady=10
        )
        self.memory_result_label.pack(fill=tk.X)

    def _create_calc_type_selector(self, parent_frame: ttk.Frame):
        type_frame = ttk.LabelFrame(parent_frame, text="计算类型", padding=10)
        type_frame.pack(fill=tk.Y)

        for value, text in CALC_TYPES.items():
            rb = tk.Radiobutton(
                type_frame,
                text=text,
                variable=self.memory_calc_type,
                value=value,
                command=self.switch_memory_calc_type,
                font=FONTS['default'],
                bg=COLORS['background'],
                activebackground=COLORS['active_bg'],
                selectcolor=COLORS['select_bg'],
                indicatoron=0,
                width=18,
                height=2,
                relief=tk.RAISED,
                bd=2
            )
            rb.pack(fill=tk.X, pady=3)

    def _create_input_field(self, parent_frame: ttk.Frame, label_text: str, name_text: str, 
                           entry_width: int, pady: int, command: Callable) -> Tuple[ttk.Entry, ttk.Label]:
        field_frame = ttk.Frame(parent_frame)
        field_frame.pack(fill=tk.X, pady=pady)

        label_frame = ttk.Frame(field_frame)
        label_frame.pack(side=tk.LEFT)

        ttk.Label(label_frame, text=label_text, width=15).pack()
        name_label = ttk.Label(label_frame, text=name_text, font=FONTS['small'], foreground='gray')
        name_label.pack()

        entry = ttk.Entry(field_frame, width=entry_width)
        entry.pack(side=tk.LEFT, padx=5)

        ttk.Button(field_frame, text="转换并查询", command=command).pack(side=tk.LEFT, padx=5)

        return entry, name_label

    def _create_result_text(self, parent: ttk.Frame) -> tk.Text:
        result_text = tk.Text(parent, height=RESULT_HEIGHT, width=RESULT_WIDTH, wrap=tk.WORD)
        result_text.pack(padx=WINDOW_PADDING * 2, pady=10, fill=tk.BOTH, expand=True)
        scroll = ttk.Scrollbar(result_text, command=result_text.yview)
        scroll.pack(side=tk.RIGHT, fill=tk.Y)
        result_text.config(yscrollcommand=scroll.set)
        return result_text

    def _configure_result_tags(self, text_widget: tk.Text, is_status: bool = False):
        text_widget.tag_configure('title', foreground=COLORS['title'], font=FONTS['title'], background=COLORS['title_bg'])
        text_widget.tag_configure('separator', foreground=COLORS['separator'])
        text_widget.tag_configure('fault', foreground=COLORS['fault'])
        if is_status:
            text_widget.tag_configure('result', foreground=COLORS['result'], font=FONTS['result'])

    def _configure_tab_styles(self):
        self.style.configure('TNotebook.Tab', 
                         font=FONTS['bold'], 
                         padding=[10, 5])
        self.style.map('TNotebook.Tab',
                     background=[('selected', COLORS['tab_selected'])],
                     foreground=[('selected', COLORS['tab_selected_text'])])

    def _on_tab_changed(self, event):
        pass

    def switch_memory_calc_type(self):
        for widget in self.memory_input_frame.winfo_children():
            widget.destroy()

        self.memory_entries = {}
        calc_type = self.memory_calc_type.get()

        calc_type_configs = {
            1: [
                ("初始内存映象网地址:", "initial_mem_address", COLORS['error']),
                ("初始R地址:", "initial_r_address", COLORS['info']),
                ("终止R地址:", "end_r_address", COLORS['text'])
            ],
            2: [
                ("初始内存映象网地址:", "initial_mem_address", COLORS['error']),
                ("终止内存映象网地址:", "end_mem_address", COLORS['text']),
                ("初始R地址:", "initial_r_address", COLORS['info'])
            ],
            3: [
                ("初始内存映象网地址:", "initial_mem_address", COLORS['error']),
                ("初始m地址:", "initial_m_address", COLORS['success']),
                ("终止m地址:", "end_m_address", COLORS['text'])
            ],
            4: [
                ("初始内存映象网地址:", "initial_mem_address", COLORS['error']),
                ("终止内存映象网地址:", "end_mem_address", COLORS['text']),
                ("初始m地址:", "initial_m_address", COLORS['success']),
                ("位编号:", "bit_position", COLORS['text'])
            ]
        }

        for label_text, key, fg_color in calc_type_configs.get(calc_type, []):
            self.create_memory_input_field(label_text, key, fg_color)

    def create_memory_input_field(self, label_text: str, key: str, fg_color: str):
        frame = ttk.Frame(self.memory_input_frame)
        frame.pack(fill=tk.X, pady=10)

        label = tk.Label(frame, text=label_text, font=FONTS['label'], width=18, anchor=tk.W, bg=COLORS['background'])
        label.pack(side=tk.LEFT)

        entry = tk.Entry(frame, font=FONTS['label'], fg=fg_color, insertbackground=fg_color, bg='white', relief=tk.SOLID, bd=1)
        entry.pack(side=tk.LEFT, fill=tk.X, expand=True, padx=(10, 0), ipady=3)
        entry.bind('<Return>', lambda e: self.on_memory_calculate())
        self.memory_entries[key] = entry

    def on_memory_calculate(self):
        try:
            calc_type = self.memory_calc_type.get()
            self._validate_memory_inputs(calc_type)
            result = self._calculate_memory_result(calc_type)
            self.memory_result_label.config(text=result)
        except ValueError as e:
            self.memory_result_label.config(text=f"错误: {e}")
        except KeyError as e:
            self.memory_result_label.config(text=f"错误: 输入框 '{e}' 不存在或未填写")

    def _validate_memory_inputs(self, calc_type: int):
        calc_type = self.memory_calc_type.get()
        required_fields = {
            1: ['initial_mem_address', 'initial_r_address', 'end_r_address'],
            2: ['initial_mem_address', 'initial_r_address', 'end_mem_address'],
            3: ['initial_mem_address', 'initial_m_address', 'end_m_address'],
            4: ['initial_mem_address', 'initial_m_address', 'end_mem_address', 'bit_position']
        }

        for field in required_fields.get(calc_type, []):
            if not self.memory_entries[field].get().strip():
                raise ValueError("所有输入框必须填写")

    def _calculate_memory_result(self, calc_type: int) -> str:
        if calc_type == 1:
            initial_mem_address = self.memory_entries['initial_mem_address'].get().upper()
            initial_r_address = int(self.memory_entries['initial_r_address'].get())
            end_r_address = int(self.memory_entries['end_r_address'].get())
            result = calculate_analog_end_mem_address(initial_mem_address, initial_r_address, end_r_address)
            return f"终止内存映象网地址: {result}"
        elif calc_type == 2:
            initial_mem_address = self.memory_entries['initial_mem_address'].get().upper()
            initial_r_address = int(self.memory_entries['initial_r_address'].get())
            end_mem_address = self.memory_entries['end_mem_address'].get().upper()
            result = calculate_analog_end_r_address(initial_mem_address, initial_r_address, end_mem_address)
            return f"终止R地址: {result}"
        elif calc_type == 3:
            initial_mem_address = self.memory_entries['initial_mem_address'].get().upper()
            initial_m_address = int(self.memory_entries['initial_m_address'].get())
            end_m_address = int(self.memory_entries['end_m_address'].get())
            result, bit_position = calculate_digital_end_mem_address_and_bit(initial_mem_address, initial_m_address, end_m_address)
            return f"终止内存映象网地址: {result}, 位编号: {bit_position}"
        elif calc_type == 4:
            initial_mem_address = self.memory_entries['initial_mem_address'].get().upper()
            initial_m_address = int(self.memory_entries['initial_m_address'].get())
            end_mem_address = self.memory_entries['end_mem_address'].get().upper()
            bit_position = int(self.memory_entries['bit_position'].get())
            result = calculate_digital_end_m_address(initial_mem_address, initial_m_address, end_mem_address, bit_position)
            return f"终止m地址: {result}"
        return ""

    def import_json(self):
        file_path = filedialog.askopenfilename(
            title="选择配置JSON文件",
            filetypes=[("JSON文件", "*.json"), ("所有文件", "*.*")],
            initialdir="."
        )
        if not file_path:
            return

        try:
            data, decode_method = self._load_json_file(file_path)
            self._process_imported_data(data, decode_method)
        except FileNotFoundError:
            messagebox.showerror("文件不存在", "请选择有效的JSON文件")
        except Exception as e:
            messagebox.showerror("未知错误", f"导入失败：{str(e)}")

    def _load_json_file(self, file_path: str) -> Tuple[Dict[str, Any], str]:
        with open(file_path, "r", encoding="utf-8") as f:
            content = f.read().strip()

        try:
            data = json.loads(content)
            return data, "明文格式"
        except json.JSONDecodeError:
            try:
                decoded_content = base64.b64decode(content).decode('utf-8')
                data = json.loads(decoded_content)
                return data, "Base64编码格式"
            except Exception as decode_error:
                raise ValueError(
                    f"无法解析文件内容。\n既不是有效的JSON格式，也不是有效的Base64编码JSON格式。\n\n错误详情：{str(decode_error)}"
                )

    def _process_imported_data(self, data: Dict[str, Any], decode_method: str):
        if "groups" in data:
            self._process_grouped_data(data["groups"], decode_method)
        else:
            self._process_ungrouped_data(data, decode_method)

    def _process_grouped_data(self, groups_data: Dict[str, Any], decode_method: str):
        group_data = groups_data
        is_status_format = any(
            "status_map" in group_data[name] or "status_name" in group_data[name]
            for name in group_data
        )

        if is_status_format:
            self.status_groups_data = groups_data
            group_names = list(self.status_groups_data.keys())
            if group_names:
                self.load_status_group(group_names[0])
        else:
            self.groups_data = groups_data
            group_names = list(self.groups_data.keys())
            self.result_text.insert(tk.END, f"成功导入 {len(group_names)} 个分组！({decode_method})\n", 'success')
            self.result_text.insert(tk.END, f"分组列表：{', '.join(group_names)}\n")
            if group_names:
                self.load_group(group_names[0])

    def _process_ungrouped_data(self, data: Dict[str, Any], decode_method: str):
        is_status_format = isinstance(data, dict) and all(isinstance(v, str) for v in data.values())

        if is_status_format:
            self.status_word_map = data
            status_keys = list(self.status_word_map.keys())
            if status_keys:
                self.load_status_word(status_keys[0])
        else:
            self._validate_ungrouped_structure(data)
            self._load_ungrouped_fault_data(data, decode_method)

    def _validate_ungrouped_structure(self, data: Dict[str, Any]):
        if not isinstance(data.get("first_word"), dict):
            raise ValueError("JSON中缺少或错误的'first_word'字段（需为对象类型）")
        if not isinstance(data.get("second_word"), dict):
            raise ValueError("JSON中缺少或错误的'second_word'字段（需为对象类型）")

    def _load_ungrouped_fault_data(self, data: Dict[str, Any], decode_method: str):
        if "first_name" in data:
            self.first_name = data["first_name"]
        if "second_name" in data:
            self.second_name = data["second_name"]

        self.first_word_map = data["first_word"]
        self.second_word_map = data["second_word"]

        self.first_name_label.config(text=f"{self.first_name}")
        self.second_name_label.config(text=f"{self.second_name}")
        self.current_group_label.config(text="当前选择的打包字：默认（无分组）")
        self.result_text.insert(tk.END, f"打包数据对应表导入完成！({decode_method})\n", 'success')

    def show_group_menu(self):
        current_tab = self.notebook.select()
        tab_text = self.notebook.tab(current_tab, "text")

        if "打包数据查看" in tab_text:
            if not self.groups_data:
                messagebox.showinfo("提示", "请先导入打包数据JSON配置文件")
                return
            self._show_popup_menu("选择打包字", list(self.groups_data.keys()), self.load_group)
        elif "状态字查看" in tab_text:
            if not self.status_groups_data and not self.status_word_map:
                messagebox.showinfo("提示", "请先导入状态字JSON配置文件")
                return
            self._show_status_menu()
        else:
            messagebox.showinfo("提示", "请在'打包数据查看'或'状态字查看'标签页中使用列表功能")

    def _show_popup_menu(self, title: str, items: List[str], load_command: Callable):
        popup = tk.Toplevel(self.root)
        popup.title(title)
        popup.resizable(False, False)
        popup.transient(self.root)
        popup.grab_set()

        main_frame = ttk.Frame(popup)
        main_frame.pack(fill=tk.BOTH, expand=True, padx=WINDOW_PADDING, pady=WINDOW_PADDING)

        canvas, scrollable_frame = self._create_scrollable_frame(main_frame)

        num_columns = 1 if len(items) <= 10 else 2

        for i, item in enumerate(items):
            row = i // num_columns
            col = i % num_columns
            btn = ttk.Button(
                scrollable_frame,
                text=item,
                command=lambda name=item, p=popup: load_command(name, p)
            )
            btn.grid(row=row, column=col, padx=5, pady=5, sticky="ew")

        for col in range(num_columns):
            scrollable_frame.columnconfigure(col, weight=1)

        self._adjust_popup_size(popup, scrollable_frame, self.group_btn)

    def _show_status_menu(self):
        popup = tk.Toplevel(self.root)
        popup.title("选择状态字")
        popup.resizable(False, False)
        popup.transient(self.root)
        popup.grab_set()

        main_frame = ttk.Frame(popup)
        main_frame.pack(fill=tk.BOTH, expand=True, padx=WINDOW_PADDING, pady=WINDOW_PADDING)

        canvas, scrollable_frame = self._create_scrollable_frame(main_frame)

        if self.status_groups_data:
            self._add_status_group_buttons(scrollable_frame, popup)
        
        if self.status_word_map and not self.status_groups_data:
            self._add_status_word_buttons(scrollable_frame, popup)

        self._adjust_popup_size(popup, scrollable_frame, self.group_btn)

    def _create_scrollable_frame(self, parent: ttk.Frame) -> Tuple[tk.Canvas, ttk.Frame]:
        canvas = tk.Canvas(parent)
        scrollbar = ttk.Scrollbar(parent, orient="vertical", command=canvas.yview)
        scrollable_frame = ttk.Frame(canvas)

        scrollable_frame.bind("<Configure>", lambda e: canvas.configure(scrollregion=canvas.bbox("all")))
        canvas.create_window((0, 0), window=scrollable_frame, anchor="nw")
        canvas.configure(yscrollcommand=scrollbar.set)

        canvas.pack(side="left", fill="both", expand=True)
        scrollbar.pack(side="right", fill="y")

        return canvas, scrollable_frame

    def _add_status_group_buttons(self, scrollable_frame: ttk.Frame, popup: tk.Toplevel):
        status_group_names = list(self.status_groups_data.keys())
        num_columns = 1 if len(status_group_names) <= 10 else 2

        for i, group_name in enumerate(status_group_names):
            row = i // num_columns
            col = i % num_columns
            btn = ttk.Button(
                scrollable_frame,
                text=group_name,
                command=lambda name=group_name, p=popup: self.load_status_group(name, p)
            )
            btn.grid(row=row, column=col, padx=5, pady=5, sticky="ew")

        for col in range(num_columns):
            scrollable_frame.columnconfigure(col, weight=1)

    def _add_status_word_buttons(self, scrollable_frame: ttk.Frame, popup: tk.Toplevel):
        status_keys = list(self.status_word_map.keys())
        num_columns = 1 if len(status_keys) <= 10 else 2

        for i, status_key in enumerate(status_keys):
            row = i // num_columns
            col = i % num_columns
            btn = ttk.Button(
                scrollable_frame,
                text=status_key,
                command=lambda key=status_key, p=popup: self.load_status_word(key, p)
            )
            btn.grid(row=row, column=col, padx=5, pady=5, sticky="ew")

        for col in range(num_columns):
            scrollable_frame.columnconfigure(col, weight=1)

    def _adjust_popup_size(self, popup: tk.Toplevel, scrollable_frame: ttk.Frame, reference_btn: Optional[ttk.Button] = None):
        popup.update_idletasks()
        content_width = scrollable_frame.winfo_reqwidth()
        content_height = scrollable_frame.winfo_reqheight()

        if content_height > WINDOW_MAX_HEIGHT:
            canvas = scrollable_frame.master
            canvas.config(height=WINDOW_MAX_HEIGHT)
            window_height = WINDOW_MAX_HEIGHT + 40
        else:
            window_height = content_height + 20

        window_width = content_width + 40
        popup.geometry(f"{window_width}x{window_height}")

        if reference_btn:
            x = reference_btn.winfo_rootx()
            y = reference_btn.winfo_rooty() + reference_btn.winfo_height()
            popup.geometry(f"+{x}+{y}")

    def load_group(self, group_name: str, popup: Optional[tk.Toplevel] = None):
        if group_name not in self.groups_data:
            messagebox.showerror("错误", f"分组 '{group_name}' 不存在")
            return

        group_data = self.groups_data[group_name]

        if not isinstance(group_data.get("first_word"), dict):
            messagebox.showerror("结构错误", f"分组 '{group_name}' 中缺少或错误的'first_word'字段")
            return
        if not isinstance(group_data.get("second_word"), dict):
            messagebox.showerror("结构错误", f"分组 '{group_name}' 中缺少或错误的'second_word'字段")
            return

        self.first_name = group_data.get("first_name", "第一个字")
        self.second_name = group_data.get("second_name", "第二个字")
        self.first_word_map = group_data["first_word"]
        self.second_word_map = group_data["second_word"]
        self.current_group = group_name

        self.first_name_label.config(text=f"{self.first_name}")
        self.second_name_label.config(text=f"{self.second_name}")
        self.current_group_label.config(text=f"当前选择的打包字：{group_name}")

        self.result_text.delete(1.0, tk.END)
        self.result_text.insert(tk.END, f"已加载打包字：{group_name}\n", 'title')
        self.result_text.insert(tk.END, f"第一个字名称：{self.first_name}\n")
        self.result_text.insert(tk.END, f"第二个字名称：{self.second_name}\n")
        self.result_text.insert(tk.END, "════════════════════════\n", 'separator')
        self.result_text.insert(tk.END, "请在输入框中输入十进制数值进行查询\n")

        if popup:
            popup.destroy()

    def decimal_to_binary(self, num: int, max_bits: Optional[int] = None) -> str:
        if num >= 0:
            binary = bin(num)[2:]
        else:
            bits = max_bits if max_bits else 64
            binary = bin((1 << bits) + num)[2:]

        if max_bits is None:
            max_key = max([int(k) for k in self.first_word_map.keys()], default=0) if self.first_word_map else 0
            max_bits = max_key + 1

        return binary.zfill(max_bits)

    def check_fault(self, word_type: int):
        self.result_text.delete(1.0, tk.END)
        try:
            if word_type == 1:
                num = int(self.first_entry.get())
                fault_map = self.first_word_map
                title = "第一个字检查结果："
            else:
                num = int(self.second_entry.get())
                fault_map = self.second_word_map
                title = "第二个字检查结果："

            max_key = max([int(k) for k in fault_map.keys()], default=0) if fault_map else 0
            binary_str = self.decimal_to_binary(num, max_key + 1)
            reversed_bits = binary_str[::-1]
            formatted_bits = ' '.join([reversed_bits[i:i+4] for i in range(0, len(reversed_bits), 4)])

            self.result_text.insert(tk.END, f"{title}\n", 'title')
            self.result_text.insert(tk.END, f"十进制值：{num}\n")
            self.result_text.insert(tk.END, f"二进制表示（低位在前）：{formatted_bits}\n")
            self.result_text.insert(tk.END, "════════════════════════\n", 'separator')
            self.result_text.insert(tk.END, "你查询的结果如下：\n", 'title')

            for bit_pos in range(len(reversed_bits)):
                if str(bit_pos) in fault_map:
                    display_on = fault_map[str(bit_pos)].get('display_on', '1')
                    current_bit = reversed_bits[bit_pos]
                    if current_bit == display_on:
                        description = fault_map[str(bit_pos)].get('description', '')
                        if description:
                            self.result_text.insert(tk.END, f"  • 第{bit_pos}位: {description}\n", 'fault')

            if self.result_text.get(1.0, tk.END).strip() == title:
                self.result_text.insert(tk.END, "无对应数据位或打包数据表未导入")

        except ValueError:
            messagebox.showerror("错误", "请输入有效的十进制整数")

    def show_about(self):
        about_text = """自动化计算工具-王国强-202603

版本：3.0
开发者：德龙轧钢自动化团队

本软件用于查看和分析打包数据及状态字，
支持导入JSON格式的配置文件，
帮助工程师快速定位和诊断设备状态。

功能说明：
• 导入：导入JSON格式的打包数据或状态字配置文件
• 列表：选择不同的打包字分组或状态字分组
• 计算器：打开系统计算器
• 关于：显示本帮助信息

支持的文件格式：
• 明文JSON格式
• Base64编码JSON格式

© 2026 德龙轧钢自动化团队"""
        messagebox.showinfo("关于", about_text)

    def load_status_group(self, group_name: str, popup: Optional[tk.Toplevel] = None):
        if group_name not in self.status_groups_data:
            messagebox.showerror("错误", f"状态字分组 '{group_name}' 不存在")
            return

        group_data = self.status_groups_data[group_name]
        status_map_key = self._get_status_map_key(group_data, group_name)
        if not status_map_key:
            return

        self.current_status_name = group_data.get("status_name", group_data.get("first_name", "状态字"))
        self.status_word_map = group_data[status_map_key]
        self.current_status_group = group_name

        self.current_status_label.config(text=f"当前选择的状态字分组：{group_name}")

        self.status_result_text.delete(1.0, tk.END)
        self.status_result_text.insert(tk.END, f"已加载状态字分组：{group_name}\n", 'title')
        self.status_result_text.insert(tk.END, f"状态字名称：{self.current_status_name}\n")
        self.status_result_text.insert(tk.END, "════════════════════════\n", 'separator')
        self.status_result_text.insert(tk.END, "状态字映射列表：\n\n", 'title')

        for key in sorted(self.status_word_map.keys(), key=lambda x: int(x) if x.isdigit() else x):
            self.status_result_text.insert(tk.END, f'        "{key}": "{self.status_word_map[key]}",\n', 'result')

        if popup:
            popup.destroy()

    def _get_status_map_key(self, group_data: Dict[str, Any], group_name: str) -> Optional[str]:
        if "status_map" in group_data:
            if not isinstance(group_data.get("status_map"), dict):
                messagebox.showerror("结构错误", f"分组 '{group_name}' 中缺少或错误的'status_map'字段")
                return None
            return "status_map"

        if "first_word" in group_data or "second_word" in group_data:
            if not isinstance(group_data.get("first_word"), dict) and not isinstance(group_data.get("second_word"), dict):
                messagebox.showerror("结构错误", f"分组 '{group_name}' 中缺少或错误的状态映射字段")
                return None
            combined_map = {}
            if isinstance(group_data.get("first_word"), dict):
                combined_map.update(group_data["first_word"])
            if isinstance(group_data.get("second_word"), dict):
                combined_map.update(group_data["second_word"])
            group_data["status_map"] = combined_map
            return "status_map"

        if isinstance(group_data, dict) and all(isinstance(v, str) for v in group_data.values()):
            group_data["status_map"] = group_data
            return "status_map"

        messagebox.showerror("结构错误", f"分组 '{group_name}' 中缺少有效的状态映射字段（status_map或first_word/second_word）")
        return None

    def load_status_word(self, status_key: str, popup: Optional[tk.Toplevel] = None):
        if status_key not in self.status_word_map:
            messagebox.showerror("错误", f"状态字 '{status_key}' 不存在")
            return

        self.current_status_word = status_key

        if self.current_status_group:
            self.current_status_label.config(text=f"当前选择的状态字分组：{self.current_status_group}")
        else:
            self.current_status_label.config(text=f"当前选择的状态字：{status_key}")

        self.status_result_text.delete(1.0, tk.END)
        self.status_result_text.insert(tk.END, f"已加载状态字：{status_key}\n", 'title')
        self.status_result_text.insert(tk.END, f"状态值：{status_key}\n")
        self.status_result_text.insert(tk.END, f"描述：{self.status_word_map[status_key]}\n", 'result')
        self.status_result_text.insert(tk.END, "════════════════════════\n", 'separator')

        if popup:
            popup.destroy()

    def open_calculator(self):
        try:
            subprocess.Popen('calc.exe')
        except Exception as e:
            messagebox.showerror("错误", f"无法打开计算器：{str(e)}")

if __name__ == "__main__":
    from ctypes import windll
    try:
        windll.shcore.SetProcessDpiAwareness(1)
    except AttributeError:
        pass
    root = tk.Tk()
    app = OPUChecker(root)
    root.mainloop()
